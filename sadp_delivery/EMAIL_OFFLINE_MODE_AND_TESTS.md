# Email: Offline Mode and Tests

**Real email delivery is not implemented/configured or tested. Email verification uses a local outbox/fake transport only.**

This is the owner's decision (ADR-007). It is a scope limitation, not a defect, and it does not block the midterm.

## Configuration

| Environment | Registered `IEmailSender` (`NewsArchive/Program.cs:111-115`) | Behaviour |
|---|---|---|
| Development | `OutboxEmailSender` | Writes one text file per mail to `App_Data/outbox` under the content root. That folder is not under `wwwroot`, so it is never served, and it is in `.gitignore`. |
| Any other environment | `DisabledEmailSender` (`NewsArchive/Email/DisabledEmailSender.cs`) | `IsEnabled = false`; `SendAsync` returns `false`; no transport is created, no connection opened, no log line per mail. One start-up warning (`Program.cs:212`). |

**What is never used:**
- `SmtpEmailSender` and `SmtpTransport` stay in the code as the dormant second concrete creator of the Factory Method example (W3). No code path registers them, even when `Email:Smtp:Host` is set.
- The `Smtp` block was removed from `appsettings.json`.
- No provider, credentials, DNS or MX setup exists.

**Behaviour per feature:**

| Feature | What happens |
|---|---|
| Newsletter sign-up | Still answers "check your inbox" whatever happens, so nobody can tell whether an address is already subscribed. Outside Development no confirmation is sent, so the sign-up cannot be completed. |
| Password reset | Still answers the same whether or not the account exists. Outside Development no code is sent, so readers cannot reset by email. |
| Digest | The admin "Send digest" button checks `DigestSender.CanSend` (`AdminController.cs:161`) and says "Email delivery is turned off on this server, so no digest was sent." No subscriber is marked as sent. |

## Evidence (all offline)

| Test | What it proves |
|---|---|
| `HardeningTests.Outside_development_mail_is_disabled_even_with_an_smtp_host_and_sends_nothing` | The test host runs in Production **with `Email:Smtp:Host=smtp.example.invalid`**. Then: `IEmailSender` is `DisabledEmailSender` and the only registration; `DigestSender.CanSend` is false; 2,000 sends to `reader{i}@example.test` all return `false` and add **no** warning or error log; exactly one start-up warning exists; a `System.Net.Sockets` event listener sees **no `ConnectStart` to ports 25, 465, 587 or 2525, or to any "smtp" host** during the sends. |
| `HardeningTests.Newsletter_double_opt_in_works_offline_through_the_private_outbox` | In Development: subscribe `newsletter…@example.test` → one file in the private outbox → its confirmation link confirms → subscribing again in Bangla does not change the language → the unsubscribe link from the file unsubscribes. The outbox folder is checked to be outside `wwwroot`. The test deletes its files and the subscriber afterwards. |
| `EmailSenderTests` (5) | The Factory Method contract: a fresh transport per mail, invalid address refused before any transport, a failing transport returns `false`, the same contract for two creators, and Development outbox content. It uses a recording fake transport and opens no network connection. |
| Smoke `tools/smoke/smoke.py` | Runs Development only and strips `Email__*`/`SMTP*` environment variables. Checks: one confirmation file, the confirmation link works, the admin digest reaches the outbox, the password-reset code from the outbox is accepted (the code is never printed), an unknown email gets the same answer and no mail. The files are deleted at the end. |

## Privacy of the local outbox

- Only synthetic recipients are used (`@example.test`) plus the sample-data demo accounts (`*.local` domains, which cannot receive mail).
- Tokens and codes are read only by the test harness. They are not printed, not logged by the script, and not copied into any document.
- The outbox lives in `App_Data`: not served, not committed.

## Limits

- The socket check covers outgoing connections that .NET's `System.Net.Sockets` reports. It cannot see native code outside .NET. The app has no such mail code: the only mail classes are the four above.
- On a public deployment, newsletter confirmation and password reset by email do not work. This is documented in the README ("Email" and "Before going live").
