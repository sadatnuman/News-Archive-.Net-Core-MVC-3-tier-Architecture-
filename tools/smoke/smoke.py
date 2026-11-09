import argparse
import concurrent.futures
import datetime
import glob
import http.cookiejar
import json
import os
import random
import re
import statistics
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SOLUTION = os.path.join(ROOT, "NewsArchiveSolution")
PROJECT = os.path.join(SOLUTION, "NewsArchive")
DLL = os.path.join(PROJECT, "bin", "Release", "net10.0", "NewsArchive.dll")
OUTBOX = os.path.join(PROJECT, "App_Data", "outbox")
SAMPLE_SQL = os.path.join(ROOT, "database", "sample-data.sql")
SAFE_DB = re.compile(r"^NewsArchive(Smoke|Test)DB[A-Za-z0-9_]{0,32}$")
LOCAL_SERVER = re.compile(r"^(localhost|\.|127\.0\.0\.1|\(localdb\))(\\[A-Za-z0-9_]+)?$", re.IGNORECASE)

results = []
base = ""


def guard(server, db):
    if not SAFE_DB.match(db) or db.lower() == "newsarchivedb":
        sys.exit(f"Refusing to use database '{db}': only NewsArchiveSmokeDB* or NewsArchiveTestDB* may be used.")
    if not LOCAL_SERVER.match(server):
        sys.exit(f"Refusing to use server '{server}': only a local SQL Server (localhost, ., 127.0.0.1, (localdb)) is allowed.")


def sql(server, query, db="master"):
    out = subprocess.run(["sqlcmd", "-S", server, "-E", "-C", "-b", "-d", db, "-h", "-1", "-W", "-Q", "SET NOCOUNT ON; " + query],
                         capture_output=True, text=True)
    if out.returncode != 0:
        sys.exit("sqlcmd failed: " + out.stdout + out.stderr)
    return out.stdout.strip()


def demo_accounts():
    with open(os.path.join(PROJECT, "appsettings.Development.json"), encoding="utf-8-sig") as f:
        accounts = json.load(f)["Site"]["DemoAccounts"]
    return {a["Role"]: (a["Email"], a["Password"]) for a in accounts}


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *args, **kwargs):
        return None


def req(method, path, body=None, token=None, opener=None, form=None, headers=None):
    h = dict(headers or {})
    data = None
    if body is not None:
        data = json.dumps(body).encode()
        h["Content-Type"] = "application/json"
    if form is not None:
        data = urllib.parse.urlencode(form, doseq=True).encode()
    if token:
        h["Authorization"] = "Bearer " + token
    r = urllib.request.Request(base + path, data=data, method=method, headers=h)
    op = opener or urllib.request.build_opener(NoRedirect)
    try:
        resp = op.open(r, timeout=120)
        return resp.status, resp.read().decode("utf-8", "replace"), resp.headers
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode("utf-8", "replace"), e.headers


def check(name, ok, info=""):
    results.append((name, bool(ok)))
    print(("PASS " if ok else "FAIL ") + name + ("" if ok else "  -> " + str(info)[:200]), flush=True)


def start_app(server, db, port):
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith(("EMAIL__", "EMAIL:", "SMTP"))}
    env["ASPNETCORE_ENVIRONMENT"] = "Development"
    env["ConnectionStrings__DbConn"] = f"Data Source={server};Initial Catalog={db};Integrated Security=True;TrustServerCertificate=True;"
    env["ASPNETCORE_URLS"] = f"http://127.0.0.1:{port}"
    log = open(os.path.join(os.environ.get("TEMP", "."), f"meridian-smoke-{port}.log"), "w", encoding="utf-8")
    proc = subprocess.Popen(["dotnet", DLL], cwd=PROJECT, env=env, stdout=log, stderr=subprocess.STDOUT)
    for _ in range(120):
        try:
            if req("GET", "/health")[0] == 200:
                return proc
        except OSError:
            pass
        if proc.poll() is not None:
            sys.exit("The app stopped during start-up; see " + log.name)
        time.sleep(1)
    proc.terminate()
    sys.exit("The app did not become healthy in 120 s; see " + log.name)


def stop_app(proc):
    proc.terminate()
    try:
        proc.wait(20)
    except subprocess.TimeoutExpired:
        proc.kill()


def login(email, password):
    s, b, _ = req("POST", "/api/auth/login", {"email": email, "password": password})
    return json.loads(b)["token"] if s == 200 else None


def form_token(html):
    m = re.search(r'name="__RequestVerificationToken" type="hidden" value="([^"]+)"', html)
    return m.group(1) if m else None


def cookie_session(email, password):
    op = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()), NoRedirect)
    _, html, _ = req("GET", "/Auth/Login", opener=op)
    s, _, _ = req("POST", "/Auth/Login", opener=op, form={"Email": email, "Password": password, "__RequestVerificationToken": form_token(html)})
    return op if s == 302 else None


def dhaka_now():
    return datetime.datetime.now(datetime.timezone.utc) + datetime.timedelta(hours=6)


def functional_checks(accounts, wait_scheduler):
    print("== Pages and barriers ==")
    for p in ["/health", "/", "/?lang=bn", "/archive", "/Article/Index?keyword=river", "/rss.xml", "/sitemap.xml",
              "/swagger/index.html", "/Auth/Login", "/offline.html"]:
        s = req("GET", p)[0]
        check(f"GET {p} -> 200", s == 200, s)
    s, b, _ = req("GET", "/Auth/Login")
    check("Development sign-in page lists 3 demo accounts", b.count("data-demo-password") == 3, b.count("data-demo-password"))
    s, _, h = req("GET", "/Admin")
    check("anonymous /Admin redirects to sign-in", s == 302 and "/Auth/Login" in (h.get("Location") or ""), s)
    check("anonymous /api/articles/pending -> 401", req("GET", "/api/articles/pending")[0] == 401)

    admin = login(*accounts["Admin"])
    author = login(*accounts["Author"])
    reader = login(*accounts["Reader"])
    check("sample admin, author and reader sign in through the API", all([admin, author, reader]))
    check("reader /api/articles/pending -> 403", req("GET", "/api/articles/pending", token=reader)[0] == 403)
    check("admin /api/articles/pending -> 200", req("GET", "/api/articles/pending", token=admin)[0] == 200)
    section = json.loads(req("GET", "/api/categories")[1])[0]["categoryId"]

    def story(title, submit=True, token=None):
        s, b, _ = req("POST", "/api/articles" + ("?submit=true" if submit else ""), {
            "title": title, "summary": "Smoke-test summary text for the review flow.",
            "content": "Smoke-test body text that is long enough to pass validation.",
            "categoryId": section, "languageCode": "en"}, token=token or author)
        return s, (json.loads(b).get("articleId") if s == 201 else b)

    print("== Editorial workflow ==")
    check("reader cannot create an article (403)", story("Reader attempt", token=reader)[0] == 403)
    s, a1 = story("Smoke approval story " + str(int(time.time())))
    check("author submits a story (201)", s == 201, a1)
    check("pending story is hidden from the public (404)", req("GET", f"/api/articles/{a1}")[0] == 404)
    check("author cannot approve (403)", req("POST", f"/api/articles/{a1}/approve", {}, token=author)[0] == 403)
    check("admin approves (204)", req("POST", f"/api/articles/{a1}/approve", {}, token=admin)[0] == 204)
    check("approved story is public (200)", req("GET", f"/api/articles/{a1}")[0] == 200)
    s, b, _ = req("POST", f"/api/articles/{a1}/reject", {"note": "late"}, token=admin)
    check("a decision on an already decided story is refused as a wrong state (409)", s == 409 and "Only pending articles" in b, (s, b))
    s, b, _ = req("GET", "/api/articles?lang=en&q=" + urllib.parse.quote("smoke approv"))
    check("published story is found by search", s == 200 and f'"articleId":{a1}' in b, s)

    s, a2 = story("Smoke rejection story")
    check("admin rejects with a note (204)", req("POST", f"/api/articles/{a2}/reject", {"note": "Needs a source."}, token=admin)[0] == 204)
    mine = [x for x in json.loads(req("GET", "/api/articles/mine", token=author)[1]) if x["articleId"] == a2]
    check("author sees the rejection note", mine and mine[0].get("reviewNote") == "Needs a source.", mine[:1])
    check("rejected story stays hidden (404)", req("GET", f"/api/articles/{a2}")[0] == 404)

    print("== Stale edits ==")
    loaded = json.loads(req("GET", f"/api/articles/{a1}")[1])
    body = {k: loaded[k] for k in ("title", "summary", "content", "categoryId", "languageCode")}
    s1 = req("PUT", f"/api/articles/{a1}", dict(body, title="Smoke edit by the first editor", version=loaded["version"]), token=admin)[0]
    s2, b2, _ = req("PUT", f"/api/articles/{a1}", dict(body, title="Smoke edit from a stale copy", version=loaded["version"]), token=admin)
    check("API: first edit saves (204), stale copy is refused (409)", s1 == 204 and s2 == 409 and "Someone else changed" in b2, (s1, s2, b2))
    s3, b3, _ = req("PUT", f"/api/articles/{a1}", body, token=admin)
    check("API: an edit without a version gets 400 that names the version field", s3 == 400 and "version" in b3, (s3, b3[:120]))
    check("API: the first edit is what readers see", json.loads(req("GET", f"/api/articles/{a1}")[1])["title"] == "Smoke edit by the first editor")

    op = cookie_session(*accounts["Admin"])
    check("admin signs in through the MVC form", op is not None)
    if op:
        _, page, _ = req("GET", f"/Article/Edit/{a1}", opener=op)
        version = re.search(r'name="Version" value="([^"]*)"', page)
        check("edit page carries the version as a hidden field", version is not None)
        fields = {"Title": "Smoke edit through the form", "Summary": body["summary"], "Content": body["content"],
                  "CategoryId": section, "LanguageCode": "en", "Version": version.group(1) if version else "",
                  "__RequestVerificationToken": form_token(page)}
        s1 = req("POST", f"/Article/Edit/{a1}", opener=op, form=fields)[0]
        s2, b2, _ = req("POST", f"/Article/Edit/{a1}", opener=op, form=dict(fields, Title="Smoke stale form edit"))
        check("MVC: first save redirects (302), the same stale form shows the conflict (200)", s1 == 302 and s2 == 200 and "Someone else changed this article" in b2, (s1, s2))

    s, a3 = story("Smoke scheduled story")
    at = (dhaka_now() + datetime.timedelta(seconds=75)).strftime("%Y-%m-%dT%H:%M:%S")
    check("admin schedules for +75 s (204)", req("POST", f"/api/articles/{a3}/approve", {"publishAt": at}, token=admin)[0] == 204)
    check("scheduled story is not public yet (404)", req("GET", f"/api/articles/{a3}")[0] == 404)
    check("scheduled list contains it", f'"articleId":{a3}' in req("GET", "/api/articles/scheduled", token=admin)[1])

    print("== Offline email: newsletter, digest, password reset (local outbox only) ==")
    existing = set(glob.glob(os.path.join(OUTBOX, "*.txt")))

    def mails_to(address, wait=0, kind=""):
        deadline = time.time() + wait
        while True:
            found = [f for f in set(glob.glob(os.path.join(OUTBOX, "*.txt"))) - existing
                     if kind in os.path.basename(f) and "To: " + address in open(f, encoding="utf-8").read()]
            if found or time.time() >= deadline:
                return found
            time.sleep(0.5)

    subscriber = f"smoke{int(time.time())}@example.test"
    op = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    _, html, _ = req("GET", "/", opener=op)
    s, _, _ = req("POST", "/newsletter/subscribe", form={"email": subscriber, "__RequestVerificationToken": form_token(html)},
                  opener=op, headers={"Accept": "application/json", "X-Requested-With": "fetch"})
    confirmation = mails_to(subscriber)
    check("subscribe returns ok and writes exactly one confirmation to the private outbox", s == 200 and len(confirmation) == 1, (s, len(confirmation)))
    token = re.search(r"/newsletter/confirm\?token=([A-Za-z0-9%]+)", open(confirmation[0], encoding="utf-8").read()).group(1) if confirmation else ""
    s, page, _ = req("GET", "/newsletter/confirm?token=" + token)
    check("the confirmation link from the outbox confirms the subscription", s == 200)

    admin_web = cookie_session(*accounts["Admin"])
    _, page, _ = req("GET", "/Admin/Newsletter", opener=admin_web)
    s, _, _ = req("POST", "/Admin/SendDigest", opener=admin_web, form={"language": "en", "__RequestVerificationToken": form_token(page)})
    digest = mails_to(subscriber, wait=30, kind="digest")
    check("admin 'send digest' writes the digest for the new subscriber to the outbox", s == 302 and len(digest) == 1, (s, len(digest)))

    reader_email = accounts["Reader"][0]
    web = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()), NoRedirect)
    _, page, _ = req("GET", "/Auth/ForgetPassword", opener=web)
    req("POST", "/Auth/ForgetPassword", opener=web, form={"email": reader_email, "__RequestVerificationToken": form_token(page)})
    reset = mails_to(reader_email)
    code = re.search(r"\b(\d{6})\b", open(reset[0], encoding="utf-8").read()).group(1) if reset else ""
    _, page, _ = req("GET", "/Auth/VerifyOtp?email=" + urllib.parse.quote(reader_email), opener=web)
    s, _, h = req("POST", "/Auth/VerifyOtp", opener=web, form={"email": reader_email, "otp": code, "__RequestVerificationToken": form_token(page)})
    check("password-reset code from the outbox is accepted (code not printed)", len(reset) == 1 and s == 302 and "ChangePassword" in (h.get("Location") or ""), (len(reset), s))
    _, page, _ = req("GET", "/Auth/ForgetPassword", opener=web)
    s, _, _ = req("POST", "/Auth/ForgetPassword", opener=web, form={"email": "nobody" + str(int(time.time())) + "@example.test", "__RequestVerificationToken": form_token(page)})
    check("an unknown email gets the same answer and no mail", s == 302 and not mails_to("nobody"), s)
    for f in set(glob.glob(os.path.join(OUTBOX, "*.txt"))) - existing:
        os.remove(f)

    print("== Withdrawal ==")
    s, a4 = story("Smoke withdrawn story")
    req("POST", f"/api/articles/{a4}/approve", {}, token=admin)
    follow = urllib.request.build_opener()
    check("story is on its page, in the list and in RSS before withdrawal", req("GET", f"/news/{a4}", opener=follow)[0] == 200
          and f'"articleId":{a4},' in req("GET", "/api/articles?lang=en&pageSize=50")[1]
          and "Smoke withdrawn story" in req("GET", "/rss.xml")[1])
    check("admin deletes it (204)", req("DELETE", f"/api/articles/{a4}", token=admin)[0] == 204)
    check("withdrawn story is gone from page, API, list and RSS", req("GET", f"/news/{a4}", opener=follow)[0] == 404
          and req("GET", f"/api/articles/{a4}")[0] == 404
          and f'"articleId":{a4},' not in req("GET", "/api/articles?lang=en&pageSize=50")[1]
          and "Smoke withdrawn story" not in req("GET", "/rss.xml")[1])

    if wait_scheduler:
        print("== Waiting for the scheduler (up to 120 s) ==")
        t0 = time.time()
        while time.time() - t0 < 120 and req("GET", f"/api/articles/{a3}")[0] != 200:
            time.sleep(5)
        check(f"scheduler published the story by itself ({int(time.time() - t0)} s)", req("GET", f"/api/articles/{a3}")[0] == 200)


EN_WORDS = ("river ferry bridge dhaka election budget school hospital flood rain cyclone farmer rice export garment port railway metro "
            "court minister council market price inflation bank loan energy power gas solar cricket football team match coach "
            "university student exam teacher health vaccine doctor clinic water road traffic police festival museum music film "
            "climate coast village city district harbour airport flight tourism hotel startup technology internet mobile software "
            "budget tax salary worker union strike factory textile jute tea fish shrimp boat monsoon heat drought dam canal").split()
BN_WORDS = ("নদী সেতু ঢাকা নির্বাচন বাজেট স্কুল হাসপাতাল বন্যা বৃষ্টি ঘূর্ণিঝড় কৃষক ধান রপ্তানি পোশাক বন্দর রেল মেট্রো আদালত মন্ত্রী "
            "বাজার দাম মূল্যস্ফীতি ব্যাংক ঋণ বিদ্যুৎ গ্যাস সৌর ক্রিকেট ফুটবল দল খেলা বিশ্ববিদ্যালয় শিক্ষার্থী পরীক্ষা শিক্ষক স্বাস্থ্য টিকা "
            "চিকিৎসক পানি সড়ক যানজট পুলিশ উৎসব জাদুঘর গান চলচ্চিত্র জলবায়ু উপকূল গ্রাম শহর জেলা বিমানবন্দর পর্যটন প্রযুক্তি "
            "ইন্টারনেট মোবাইল কর বেতন শ্রমিক কারখানা পাট চা মাছ চিংড়ি নৌকা বর্ষা খরা খাল").split()
EN_SYLLABLES = "ba ka ri to men sa lo ver dan ta mi ro ne gal pur shi ha bon tor lin".split()
BN_SYLLABLES = "ক খ গ চ জ ত দ ন প ব ম র ল স হ কা বি নু রে মো".split()


def vocabulary(rng, words, syllables, size):
    vocab = list(words)
    seen = set(vocab)
    while len(vocab) < size:
        w = "".join(rng.choice(syllables) for _ in range(rng.randint(2, 4)))
        if w not in seen:
            seen.add(w)
            vocab.append(w)
    weights = [1 / (rank + 1) for rank in range(len(vocab))]
    return vocab, weights


def generate(accounts, count, seed):
    rng = random.Random(seed)
    langs = {"en": vocabulary(rng, EN_WORDS, EN_SYLLABLES, 3000), "bn": vocabulary(rng, BN_WORDS, BN_SYLLABLES, 3000)}
    sections = [c["categoryId"] for c in json.loads(req("GET", "/api/categories")[1])]

    def text(lang, n):
        vocab, weights = langs[lang]
        return " ".join(rng.choices(vocab, weights, k=n))

    jobs = []
    for i in range(count):
        lang = "en" if i % 2 == 0 else "bn"
        title = (text(lang, rng.randint(6, 10)) + f" {i}")[:190]
        summary = text(lang, rng.randint(20, 35))[:480]
        paragraphs = [text(lang, rng.randint(40, 90)) for _ in range(rng.randint(3, 6))]
        jobs.append({"Title": title, "Summary": summary, "Content": "\n\n".join(paragraphs)[:19000],
                     "CategoryId": rng.choice(sections), "LanguageCode": lang})

    op = cookie_session(*accounts["Admin"])
    token = form_token(req("GET", "/Article/Create", opener=op)[1])

    def create(fields):
        return req("POST", "/Article/Create", opener=op, form=dict(fields, __RequestVerificationToken=token))[0]

    t0 = time.time()
    with concurrent.futures.ThreadPoolExecutor(8) as pool:
        codes = list(pool.map(create, jobs))
    created = codes.count(302)
    print(f"GENERATED {created}/{count} stories through the admin form in {time.time() - t0:.0f} s "
          f"(status counts: {dict((c, codes.count(c)) for c in sorted(set(codes)))})", flush=True)
    return langs, created


def pct(values, p):
    values = sorted(values)
    k = max(0, min(len(values) - 1, int(round(p / 100 * len(values) + 0.5)) - 1))
    return values[k]


def queries(langs, seed):
    rng = random.Random(seed + 1)
    out = []
    for lang in ("en", "bn"):
        vocab, _ = langs[lang]
        out += [vocab[rng.randrange(0, 10)] for _ in range(3)]
        out += [vocab[rng.randrange(50, 200)] for _ in range(3)]
        out += [vocab[rng.randrange(1000, 3000)] for _ in range(3)]
        out += [vocab[rng.randrange(0, 60)] + " " + vocab[rng.randrange(0, 60)] for _ in range(3)]
        out += [vocab[rng.randrange(0, 60)][:3] for _ in range(2)]
    out += ["zzqxnomatch", "নেইএমনশব্দ"]
    return out


def benchmark(query_list, sequential, concurrent_requests, workers):
    def timed(path):
        t = time.perf_counter()
        try:
            s = req("GET", path)[0]
        except OSError:
            s = 0
        return (time.perf_counter() - t) * 1000, s

    def report(label, samples, wall):
        ok = [m for m, s in samples if s == 200]
        errors = len(samples) - len(ok)
        if not ok:
            print(f"BENCH {label} n={len(samples)} all requests failed", flush=True)
            return errors
        print(f"BENCH {label} n={len(samples)} p50={statistics.median(ok):.1f}ms p95={pct(ok, 95):.1f}ms "
              f"p99={pct(ok, 99):.1f}ms throughput={len(ok) / wall:.1f} req/s errors={errors}", flush=True)
        return errors

    pages = [f"/Article/Index?keyword={urllib.parse.quote(q)}" for q in query_list]
    for p in pages[:20]:
        timed(p)
    t = time.perf_counter()
    samples = [timed(pages[i % len(pages)]) for i in range(sequential)]
    errors = report("search page sequential   ", samples, time.perf_counter() - t)
    t = time.perf_counter()
    with concurrent.futures.ThreadPoolExecutor(workers) as pool:
        samples = list(pool.map(lambda i: timed(pages[i % len(pages)]), range(concurrent_requests)))
    errors += report(f"search page concurrent={workers}", samples, time.perf_counter() - t)
    for label, path in (("news sitemap             ", "/news-sitemap.xml"), ("sitemap                  ", "/sitemap.xml")):
        t = time.perf_counter()
        samples = [timed(path) for _ in range(50)]
        errors += report(label, samples, time.perf_counter() - t)
    return errors


def main():
    global base
    parser = argparse.ArgumentParser(description="Safe end-to-end smoke test and search benchmark on a disposable database.")
    parser.add_argument("--server", default=r"localhost\SQLEXPRESS")
    parser.add_argument("--db", default="NewsArchiveSmokeDB")
    parser.add_argument("--port", type=int, default=5199)
    parser.add_argument("--stories", type=int, default=0, help="extra generated stories for the search benchmark")
    parser.add_argument("--seed", type=int, default=4273)
    parser.add_argument("--keep-db", action="store_true", help="reuse the existing smoke database instead of recreating it")
    parser.add_argument("--skip-scheduler", action="store_true")
    args = parser.parse_args()
    guard(args.server, args.db)
    base = f"http://127.0.0.1:{args.port}"

    subprocess.run(["dotnet", "build", os.path.join(SOLUTION, "NewsArchiveSolution.slnx"), "-c", "Release", "-v", "q"], check=True)
    if not args.keep_db:
        sql(args.server, f"IF DB_ID(N'{args.db}') IS NOT NULL BEGIN ALTER DATABASE [{args.db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{args.db}]; END")
        stop_app(start_app(args.server, args.db, args.port))
        out = subprocess.run(["sqlcmd", "-S", args.server, "-E", "-C", "-b", "-d", args.db, "-f", "65001", "-i", SAMPLE_SQL], capture_output=True, text=True)
        if out.returncode != 0:
            sys.exit("Sample data failed: " + out.stdout + out.stderr)

    proc = start_app(args.server, args.db, args.port)
    try:
        accounts = demo_accounts()
        functional_checks(accounts, not args.skip_scheduler)
        errors = 0
        if args.stories > 0:
            print("== Search benchmark ==")
            langs, _ = generate(accounts, args.stories, args.seed)
            counts = sql(args.server, "SELECT COUNT(*) FROM Articles WHERE StatusId = 3; SELECT COUNT(*) FROM Articles; SELECT COUNT(*) FROM ArticleWords;", args.db).split()
            print(f"DATASET published={counts[0]} total={counts[1]} word_rows={counts[2]}", flush=True)
            errors = benchmark(queries(langs, args.seed), 400, 800, 20)
        passed = sum(ok for _, ok in results)
        print(f"SUMMARY {passed}/{len(results)} checks passed; benchmark errors={errors}")
        return 0 if passed == len(results) and errors == 0 else 1
    finally:
        stop_app(proc)


if __name__ == "__main__":
    sys.exit(main())
