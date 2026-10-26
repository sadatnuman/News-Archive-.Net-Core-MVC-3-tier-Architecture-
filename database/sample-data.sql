SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DELETE FROM [ReadingHistory];
DELETE FROM [Bookmarks];
DELETE FROM [UserTokens];
DELETE FROM [ContentLabels];
DELETE FROM [Comments];
DELETE FROM [Articles];
DELETE FROM [Tags];
DELETE FROM [Categories];
DELETE FROM [Users];

DBCC CHECKIDENT ('[Users]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Categories]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Tags]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Articles]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[Comments]', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[UserTokens]', RESEED, 0) WITH NO_INFOMSGS;

SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (1, N'Editorial Desk', N'admin@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 1, '2026-07-28T09:00:00');
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (2, N'Farhana Rahman', N'farhana@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 3, '2026-07-29T09:00:00');
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (3, N'Tanvir Ahmed', N'tanvir@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 3, '2026-07-30T09:00:00');
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (4, N'Nusrat Jahan', N'nusrat@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 2, '2026-07-31T09:00:00');
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (5, N'Imran Hossain', N'imran@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 2, '2026-08-01T09:00:00');
INSERT INTO [Users] ([UserId], [Name], [Email], [PasswordHash], [RoleId], [CreatedAt]) VALUES (6, N'Sadia Islam', N'sadia@meridiannewsfolio.local', '492c58498f3fdffb6261ce8f3272edab', 2, '2026-08-02T09:00:00');
SET IDENTITY_INSERT [Users] OFF;

SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (1, N'Bangladesh', N'News from across Bangladesh: cities, districts, infrastructure and public life.', N'বাংলাদেশ', N'সারা দেশের সংবাদ: নগর, জেলা, অবকাঠামো ও জনজীবন।', N'bangladesh');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (2, N'World', N'International affairs, diplomacy and major events from around the globe.', N'বিশ্ব', N'আন্তর্জাতিক সম্পর্ক, কূটনীতি ও বিশ্বের গুরুত্বপূর্ণ ঘটনা।', N'world');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (3, N'Politics', N'Government policy, elections, parliament and public administration.', N'রাজনীতি', N'সরকারি নীতি, নির্বাচন, সংসদ ও জনপ্রশাসন।', N'politics');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (4, N'Business', N'Markets, banking, startups, trade and the wider economy.', N'অর্থনীতি', N'বাজার, ব্যাংকিং, স্টার্টআপ, বাণিজ্য ও সামগ্রিক অর্থনীতি।', N'business');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (5, N'Technology', N'Software, artificial intelligence, cybersecurity and the digital economy.', N'প্রযুক্তি', N'সফটওয়্যার, কৃত্রিম বুদ্ধিমত্তা, সাইবার নিরাপত্তা ও ডিজিটাল অর্থনীতি।', N'technology');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (6, N'Education', N'Schools, universities, exams, scholarships and learning.', N'শিক্ষা', N'স্কুল, বিশ্ববিদ্যালয়, পরীক্ষা, বৃত্তি ও শিক্ষাব্যবস্থা।', N'education');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (7, N'Sports', N'Cricket, football and other sports, with results and analysis.', N'খেলা', N'ক্রিকেট, ফুটবলসহ সব খেলার ফলাফল ও বিশ্লেষণ।', N'sports');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (8, N'Entertainment', N'Film, television, music and culture.', N'বিনোদন', N'চলচ্চিত্র, টেলিভিশন, সংগীত ও সংস্কৃতি।', N'entertainment');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (9, N'Health', N'Medicine, public health, fitness and wellbeing.', N'স্বাস্থ্য', N'চিকিৎসা, জনস্বাস্থ্য, সুস্থতা ও শরীরচর্চা।', N'health');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (10, N'Environment', N'Climate, conservation, energy and natural disasters.', N'পরিবেশ', N'জলবায়ু, প্রকৃতি সংরক্ষণ, জ্বালানি ও প্রাকৃতিক দুর্যোগ।', N'environment');
INSERT INTO [Categories] ([CategoryId], [Name], [Description], [NameBn], [DescriptionBn], [Slug]) VALUES (11, N'Science', N'Research, discoveries and space exploration.', N'বিজ্ঞান', N'গবেষণা, আবিষ্কার ও মহাকাশ অভিযান।', N'science');
SET IDENTITY_INSERT [Categories] OFF;

SET IDENTITY_INSERT [Tags] ON;
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (1, N'Dhaka', N'ঢাকা');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (2, N'Chattogram', N'চট্টগ্রাম');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (3, N'Climate', N'জলবায়ু');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (4, N'Renewable Energy', N'নবায়নযোগ্য জ্বালানি');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (5, N'Elections', N'নির্বাচন');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (6, N'Startups', N'স্টার্টআপ');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (7, N'Markets', N'বাজার');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (8, N'Exports', N'রপ্তানি');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (9, N'Universities', N'বিশ্ববিদ্যালয়');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (10, N'Cricket', N'ক্রিকেট');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (11, N'Football', N'ফুটবল');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (12, N'Film', N'চলচ্চিত্র');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (13, N'Public Health', N'জনস্বাস্থ্য');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (14, N'Space', N'মহাকাশ');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (15, N'Research', N'গবেষণা');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (16, N'Artificial Intelligence', N'কৃত্রিম বুদ্ধিমত্তা');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (17, N'Cybersecurity', N'সাইবার নিরাপত্তা');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (18, N'Transport', N'যোগাযোগ');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (19, N'Diplomacy', N'কূটনীতি');
INSERT INTO [Tags] ([TagId], [Name], [NameBn]) VALUES (20, N'Agriculture', N'কৃষি');
SET IDENTITY_INSERT [Tags] OFF;

SET IDENTITY_INSERT [Articles] ON;
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (1, N'Dhaka commuters welcome longer evening hours on city rail services', NULL, N'Rail operators in the capital have extended evening services on busy weekdays, giving office workers and students more time to get home without long road journeys.', N'Commuters in Dhaka are getting more time to travel home after rail operators extended evening services on weekdays.

Passengers interviewed at several stations said the later trains help them avoid the worst of the evening traffic, especially on days when meetings or classes run late.

Operators said they will watch passenger numbers over the coming weeks before deciding whether to keep the new timetable permanently.

Transport planners say reliable evening services are an important part of encouraging more people to leave cars and ride-hailing trips behind.', '2026-10-25T05:24:00', 2380, 1, N'images/articles/01-en-bangladesh.jpg', 1, 2, 3, 'en', '2026-10-25T03:24:00', NULL, 1, '2026-10-25T05:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (2, N'Chattogram port speeds up container checks with new digital booking system', NULL, N'Importers and exporters can now book inspection slots online at the port, cutting the time containers wait in yards, according to traders who have used the system.', N'Traders using Chattogram port say a new online booking system for container inspections is reducing the time their goods sit in the yard.

Instead of queuing for an inspection slot, agents now choose a time online and receive a confirmation message.

Garment exporters said faster checks matter most in the weeks before major shipping deadlines, when even a short delay can mean a missed vessel.

Port users asked for the system to be extended to more document checks so that paper forms can be phased out completely.', '2026-10-24T06:36:00', 1460, 0, N'images/articles/02-en-bangladesh.jpg', 1, 3, 3, 'en', '2026-10-24T04:36:00', NULL, 1, '2026-10-24T06:36:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (3, N'Farmers in the north turn to drought-tolerant rice as dry spells grow longer', NULL, N'Growers in northern districts are planting rice varieties that need less water, after several seasons of late and uneven rainfall.', N'Farmers in northern Bangladesh are switching to rice varieties that can survive longer dry spells, agricultural extension workers say.

The new varieties mature faster and need fewer irrigation rounds, which also lowers fuel costs for diesel pumps.

Some farmers remain cautious and are planting the new seeds on part of their land only, keeping traditional varieties on the rest.

Extension officers are running field demonstrations so that neighbours can compare yields before the next season.', '2026-10-22T04:12:00', 980, 0, N'images/articles/03-en-bangladesh.jpg', 1, 2, 3, 'en', '2026-10-22T02:12:00', NULL, 1, '2026-10-22T04:12:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (4, N'Bangladeshi fintech startups attract new funding for rural digital payments', NULL, N'Investors are backing startups that bring mobile wallets and small loans to rural shops, according to founders who closed funding rounds this quarter.', N'Fintech startups that serve rural shopkeepers are attracting fresh investment, founders say.

Their apps let small shops accept digital payments, track sales and apply for short-term stock loans from a phone.

Founders said investors now ask detailed questions about profits and repayment rates, and favour companies with steady revenue.

Industry groups want simpler rules for digital lending so that more small businesses can borrow safely.', '2026-10-23T18:36:00', 1320, 0, N'images/articles/04-en-business.jpg', 4, 3, 3, 'en', '2026-10-23T16:36:00', NULL, 1, '2026-10-23T18:36:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (5, N'Bangladesh name three uncapped players in squad for tri-series', NULL, N'Selectors have named a fifteen-player squad for next month''s tri-series, with three uncapped players called up after strong domestic seasons.', N'Bangladesh''s selectors have announced a fifteen-player squad for the upcoming tri-series, including three uncapped players.

The selection panel said the squad balances experience with youth and gives the captain options for different pitch conditions.

Two fast bowlers return after recovering from injury, while the top order is expected to stay largely unchanged.

The squad will gather for a two-week camp, including practice matches under lights to prepare for day-night games.', '2026-10-24T23:24:00', 2950, 1, N'images/articles/05-en-sports.jpg', 7, 2, 3, 'en', '2026-10-24T21:24:00', NULL, 1, '2026-10-24T23:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (6, N'Universities expand evening degree programmes for working students', NULL, N'Several universities are adding evening and weekend programmes after a sharp rise in applications from students who also hold full-time jobs.', N'Universities are expanding evening and weekend degree programmes after applications from working students rose over the past two years.

The programmes combine classroom sessions with recorded lectures, so students can catch up after shifts or family duties.

Administrators said demand is strongest in business, computing and public health.

Student groups asked for libraries and support services to stay open later so evening students get the same help.', '2026-10-20T21:00:00', 740, 0, N'images/articles/06-en-education.jpg', 6, 3, 3, 'en', '2026-10-20T19:00:00', NULL, 1, '2026-10-20T21:00:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (7, N'Coastal nations agree joint early-warning plan for extreme storms', NULL, N'A group of coastal nations has agreed to share storm data, use common warning levels in ports and jointly fund early-warning stations along busy shipping routes.', N'Delegates from a regional group of coastal nations have agreed a joint plan to warn ships and coastal communities earlier about extreme storms.

Weather agencies will share real-time data, and ports will use a common set of warning levels so operators receive the same message wherever they dock.

A shared fund will pay for buoys, coastal radar and training. Smaller members said the arrangement gives them tools they could not afford alone.

The plan still needs approval from national governments, with the first stations expected before next storm season.', '2026-10-24T16:12:00', 1720, 1, N'images/articles/07-en-world.jpg', 2, 2, 3, 'en', '2026-10-24T14:12:00', NULL, 1, '2026-10-24T16:12:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (8, N'New cross-border rail freight link halves journey times', NULL, N'Early figures from a new freight rail connection between neighbouring countries show goods moving in about half the time taken by road.', N'The first weeks of service on a new cross-border freight rail link show journey times roughly halved compared with trucks.

Customs officers inspect sealed containers at inland terminals instead of at the border, removing long queues.

Exporters of perishable goods said faster journeys mean less spoilage and better prices.

Operators plan more weekly trains once additional wagons arrive.', '2026-10-19T13:48:00', 650, 0, N'images/articles/08-en-world.jpg', 2, 3, 3, 'en', '2026-10-19T11:48:00', NULL, 1, '2026-10-19T13:48:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (9, N'Software teams adopt AI coding assistants but keep humans in charge of review', NULL, N'Developers say AI assistants now draft tests and explain old code, but every change still goes through human review before it ships.', N'Software companies report that AI coding assistants have moved from experiments to everyday tools.

Teams use them to draft unit tests, summarise pull requests and explain unfamiliar parts of older codebases.

Engineering leads said suggestions can be confidently wrong or ignore security rules, so code review remains mandatory.

Several companies now run internal workshops on checking generated code and keeping sensitive data out of external tools.', '2026-10-24T11:24:00', 1880, 0, N'images/articles/09-en-technology.jpg', 5, 2, 3, 'en', '2026-10-24T09:24:00', NULL, 1, '2026-10-24T11:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (10, N'Banks urge customers to enable two-step verification after phishing wave', NULL, N'A rise in fake messages asking for one-time passwords has prompted banks to remind customers that staff never ask for codes.', N'Banks have issued fresh warnings after customers reported text messages that imitate official alerts and ask for one-time passwords.

Customers are asked to enable two-step verification, type the bank''s address directly and report suspicious messages.', '2026-10-25T06:36:00', 0, 0, N'images/articles/10-en-technology.jpg', 5, 3, 2, 'en', '2026-10-25T04:36:00', NULL, NULL, NULL, NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (11, N'Clinics extend evening hours for seasonal vaccination campaign', NULL, N'Public clinics will open in the evenings and at weekends during this year''s seasonal vaccination drive to reach people who cannot attend during working hours.', N'Public clinics will stay open in the evenings and at weekends during the seasonal vaccination campaign.

Older people, pregnant women and people with long-term illnesses are being prioritised, and mobile teams will visit remote areas.

Health workers will also offer blood pressure checks during visits, reaching people who rarely see a doctor.', '2026-10-22T23:24:00', 1010, 0, N'images/articles/11-en-health.jpg', 9, 2, 3, 'en', '2026-10-22T21:24:00', NULL, 1, '2026-10-22T23:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (12, N'Walking every day linked to better sleep, readers say', NULL, N'Readers who took part in an informal survey said a daily walk helped them sleep better.', N'Readers who answered an informal survey said a daily thirty-minute walk helped them fall asleep faster.

Many said morning walks worked better than late evening walks.', '2026-10-25T01:48:00', 0, 0, N'images/articles/12-en-health.jpg', 9, 3, 4, 'en', '2026-10-24T23:48:00', NULL, 1, '2026-10-25T01:48:00', N'Please add evidence from published research or a health professional before resubmitting.');
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (13, N'Rooftop solar on public schools cuts electricity bills by a third', NULL, N'Schools that installed rooftop solar panels last year have reduced their electricity bills by about a third, and the scheme will now expand.', N'Public schools with rooftop solar panels have cut their electricity bills by around a third in the first full year.

The panels produce most of their power during school hours, when demand is highest.

Teachers use the systems in science lessons, with displays showing how much energy the panels produce each day.

The scheme will expand to more districts, prioritising schools that suffer frequent power cuts.', '2026-10-24T01:48:00', 1240, 0, N'images/articles/13-en-environment.jpg', 10, 2, 3, 'en', '2026-10-23T23:48:00', NULL, 1, '2026-10-24T01:48:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (14, N'Student-built satellite sends its first images from orbit', NULL, N'A small satellite designed and built by university students has sent its first images back to Earth.', N'A small satellite built by university students has sent its first images from orbit, weeks after launch on a shared rocket mission.

The satellite carries a camera for monitoring crops and water bodies, and students track it from a campus ground station.

The team spent three years designing and testing it, including vibration and temperature tests.

The images will be shared with other researchers, and a second satellite with a sharper camera is planned.', '2026-10-24T09:00:00', 1690, 0, N'images/articles/14-en-science.jpg', 11, 3, 3, 'en', '2026-10-24T07:00:00', NULL, 1, '2026-10-24T09:00:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (15, N'ঢাকায় সন্ধ্যার রেলসেবা বাড়ানোয় স্বস্তিতে যাত্রীরা', NULL, N'রাজধানীতে কর্মদিবসে সন্ধ্যার রেলসেবার সময় বাড়ানো হয়েছে। এতে অফিসফেরত কর্মী ও শিক্ষার্থীরা যানজট এড়িয়ে বাড়ি ফিরতে পারছেন।', N'কর্মদিবসে সন্ধ্যার রেলসেবার সময় বাড়ানোয় ঢাকার যাত্রীরা বাড়ি ফেরার জন্য আরও বেশি সময় পাচ্ছেন।

বিভিন্ন স্টেশনে কথা বলা যাত্রীরা জানান, শেষ ট্রেন দেরিতে ছাড়ায় সন্ধ্যার তীব্র যানজট এড়ানো সহজ হয়েছে, বিশেষ করে যেদিন অফিস বা ক্লাস দেরিতে শেষ হয়।

পরিচালনাকারীরা জানিয়েছেন, আগামী কয়েক সপ্তাহ যাত্রীসংখ্যা পর্যবেক্ষণ করে নতুন সময়সূচি স্থায়ী করা হবে কি না, সে সিদ্ধান্ত নেওয়া হবে।

পরিবহন পরিকল্পনাবিদদের মতে, নির্ভরযোগ্য সন্ধ্যার সেবা মানুষকে ব্যক্তিগত গাড়ি ছেড়ে গণপরিবহনে উৎসাহিত করে।', '2026-10-25T04:12:00', 2610, 1, N'images/articles/15-bn-bangladesh.jpg', 1, 2, 3, 'bn', '2026-10-25T02:12:00', NULL, 1, '2026-10-25T04:12:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (16, N'চট্টগ্রাম বন্দরে অনলাইন বুকিংয়ে কনটেইনার পরীক্ষায় গতি', NULL, N'বন্দরে কনটেইনার পরীক্ষার সময় এখন অনলাইনে বুক করা যাচ্ছে। ব্যবসায়ীরা বলছেন, এতে ইয়ার্ডে পণ্যের অপেক্ষার সময় কমেছে।', N'চট্টগ্রাম বন্দরে কনটেইনার পরীক্ষার জন্য নতুন অনলাইন বুকিং ব্যবস্থা চালু হওয়ায় পণ্য খালাসের সময় কমছে বলে জানিয়েছেন ব্যবহারকারীরা।

আগে পরীক্ষার জন্য লাইনে দাঁড়াতে হতো। এখন এজেন্টরা অনলাইনে সময় বেছে নিয়ে নিশ্চিতকরণ বার্তা পাচ্ছেন।

তৈরি পোশাক রপ্তানিকারকেরা বলছেন, জাহাজ ছাড়ার আগের ব্যস্ত সপ্তাহগুলোতে দ্রুত পরীক্ষা সবচেয়ে বেশি কাজে আসে।

বন্দর ব্যবহারকারীরা অন্যান্য কাগজপত্র যাচাইও এই ব্যবস্থায় আনার দাবি জানিয়েছেন।', '2026-10-24T13:48:00', 1540, 0, N'images/articles/16-bn-bangladesh.jpg', 1, 3, 3, 'bn', '2026-10-24T11:48:00', NULL, 1, '2026-10-24T13:48:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (17, N'খরাসহিষ্ণু ধানে ঝুঁকছেন উত্তরাঞ্চলের কৃষকেরা', NULL, N'কয়েক মৌসুম ধরে অনিয়মিত বৃষ্টির কারণে উত্তরাঞ্চলের কৃষকেরা কম পানিতে ফলন দেয় এমন ধানের জাত চাষ করছেন।', N'দীর্ঘ খরার সঙ্গে মানিয়ে নিতে উত্তরাঞ্চলের কৃষকেরা খরাসহিষ্ণু ধানের জাত বেছে নিচ্ছেন বলে জানিয়েছেন কৃষি সম্প্রসারণকর্মীরা।

নতুন জাতের ধান দ্রুত পাকে এবং কম সেচ লাগে, ফলে ডিজেলচালিত পাম্পের খরচও কমে।

কিছু কৃষক অবশ্য সতর্ক। তাঁরা জমির একটি অংশে নতুন জাত আর বাকি অংশে পুরোনো জাত চাষ করছেন।

পরের মৌসুমের আগে প্রতিবেশীরা যেন ফলন তুলনা করতে পারেন, সে জন্য মাঠ প্রদর্শনী করছেন সম্প্রসারণ কর্মকর্তারা।', '2026-10-22T16:12:00', 1120, 0, N'images/articles/17-bn-bangladesh.jpg', 1, 2, 3, 'bn', '2026-10-22T14:12:00', NULL, 1, '2026-10-22T16:12:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (18, N'উপকূল রক্ষায় ম্যানগ্রোভের চারা রোপণে স্বেচ্ছাসেবকেরা', NULL, N'ভাঙনপ্রবণ একটি উপকূলীয় এলাকায় শত শত স্বেচ্ছাসেবক ম্যানগ্রোভের চারা রোপণ করেছেন। দীর্ঘ মেয়াদে ঝড়ের ক্ষতি কমানোই লক্ষ্য।', N'সপ্তাহান্তে শত শত স্বেচ্ছাসেবক একটি ভাঙনপ্রবণ উপকূলে ম্যানগ্রোভের চারা রোপণ করেছেন।

ম্যানগ্রোভের শিকড় পলি ধরে রাখে এবং ঢেউয়ের শক্তি কমায়, ফলে ঝড়ের সময় গ্রামগুলো কিছুটা সুরক্ষা পায়।

স্থানীয় সংগঠনগুলো দুই বছর চারাগুলোর যত্ন নেবে এবং মারা যাওয়া চারার জায়গায় নতুন চারা লাগাবে।', '2026-10-19T06:36:00', 690, 0, N'images/articles/18-bn-bangladesh.jpg', 1, 3, 3, 'bn', '2026-10-19T04:36:00', NULL, 1, '2026-10-19T06:36:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (19, N'গ্রামের দোকানে ডিজিটাল লেনদেনে বিনিয়োগ পাচ্ছে স্টার্টআপ', NULL, N'গ্রামের ছোট দোকানে মোবাইল ওয়ালেট ও স্বল্পমেয়াদি ঋণসেবা পৌঁছে দেওয়া স্টার্টআপগুলো এই প্রান্তিকে নতুন বিনিয়োগ পেয়েছে।', N'গ্রামের দোকানদারদের জন্য ডিজিটাল সেবা দেওয়া ফিনটেক স্টার্টআপগুলো নতুন বিনিয়োগ আকর্ষণ করছে বলে জানিয়েছেন উদ্যোক্তারা।

তাদের অ্যাপের মাধ্যমে দোকানিরা ডিজিটাল পেমেন্ট নিতে, বিক্রির হিসাব রাখতে এবং মালামাল কেনার জন্য স্বল্পমেয়াদি ঋণের আবেদন করতে পারেন।

উদ্যোক্তারা বলছেন, বিনিয়োগকারীরা এখন মুনাফা ও ঋণ পরিশোধের হার নিয়ে খুঁটিনাটি প্রশ্ন করেন।

শিল্প সংগঠনগুলো ডিজিটাল ঋণের নিয়ম সহজ করার দাবি জানিয়েছে।', '2026-10-23T11:24:00', 1280, 0, N'images/articles/19-bn-business.jpg', 4, 2, 3, 'bn', '2026-10-23T09:24:00', NULL, 1, '2026-10-23T11:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (20, N'ত্রিদেশীয় সিরিজের দলে তিন নতুন মুখ', NULL, N'আগামী মাসের ত্রিদেশীয় সিরিজের জন্য ১৫ সদস্যের দল ঘোষণা করেছেন নির্বাচকেরা। ঘরোয়া ক্রিকেটে ভালো খেলে দলে জায়গা পেয়েছেন তিন নতুন খেলোয়াড়।', N'ত্রিদেশীয় সিরিজের জন্য ১৫ সদস্যের দল ঘোষণা করা হয়েছে, যেখানে প্রথমবারের মতো ডাক পেয়েছেন তিন ক্রিকেটার।

নির্বাচকেরা জানিয়েছেন, অভিজ্ঞতা আর তারুণ্যের মিশেলে দলটি গড়া হয়েছে, যাতে বিভিন্ন উইকেটে অধিনায়কের হাতে বিকল্প থাকে।

চোট কাটিয়ে দলে ফিরেছেন দুই পেসার। ব্যাটিং অর্ডারে বড় কোনো পরিবর্তনের সম্ভাবনা নেই।

দিবা-রাত্রির ম্যাচের প্রস্তুতি হিসেবে দুই সপ্তাহের ক্যাম্পে ফ্লাডলাইটের নিচে অনুশীলন ম্যাচ খেলবে দল।', '2026-10-24T21:00:00', 3120, 1, N'images/articles/20-bn-sports.jpg', 7, 3, 3, 'bn', '2026-10-24T19:00:00', NULL, 1, '2026-10-24T21:00:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (21, N'কর্মজীবী শিক্ষার্থীদের জন্য সান্ধ্য কোর্স বাড়াচ্ছে বিশ্ববিদ্যালয়', NULL, N'চাকরির পাশাপাশি পড়তে চান এমন শিক্ষার্থীদের আবেদন বাড়ায় কয়েকটি বিশ্ববিদ্যালয় সান্ধ্য ও সাপ্তাহিক ছুটির দিনের কোর্স চালু করছে।', N'চাকরিজীবী শিক্ষার্থীদের আবেদন বাড়ায় বিশ্ববিদ্যালয়গুলো সান্ধ্য ও ছুটির দিনের ডিগ্রি কোর্সের সংখ্যা বাড়াচ্ছে।

এসব কোর্সে সরাসরি ক্লাসের পাশাপাশি রেকর্ড করা লেকচার থাকে, ফলে কাজ শেষে শিক্ষার্থীরা পড়া পুষিয়ে নিতে পারেন।

শিক্ষার্থী সংগঠনগুলো সন্ধ্যার শিক্ষার্থীদের জন্য গ্রন্থাগার ও সহায়তা সেবা আরও দেরি পর্যন্ত খোলা রাখার অনুরোধ জানিয়েছে।', '2026-10-21T18:36:00', 820, 0, N'images/articles/21-bn-education.jpg', 6, 2, 3, 'bn', '2026-10-21T16:36:00', NULL, 1, '2026-10-21T18:36:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (22, N'রেকর্ডসংখ্যক ছবি নিয়ে শুরু হলো স্বাধীন চলচ্চিত্র উৎসব', NULL, N'এ বছরের স্বাধীন চলচ্চিত্র উৎসবে রেকর্ডসংখ্যক ছবি জমা পড়েছে। নতুন নির্মাতা ও প্রামাণ্যচিত্রের অংশগ্রহণ এবার সবচেয়ে বেশি।', N'রেকর্ডসংখ্যক ছবি জমা পড়ার মধ্য দিয়ে এ সপ্তাহে শুরু হয়েছে বার্ষিক স্বাধীন চলচ্চিত্র উৎসব।

প্রামাণ্যচিত্র বিভাগে সবচেয়ে বেশি ছবি এসেছে। নির্বাচিত ছবির এক-তৃতীয়াংশের বেশি নির্মাণ করেছেন প্রথমবারের নির্মাতারা।

উৎসবে এবার উন্মুক্ত প্রদর্শনী যোগ হয়েছে, যাতে সাধারণ দর্শকেরাও বিনা মূল্যে ছবি দেখতে পারেন।

সমাপনী রাতে মোবাইল ফোনে নির্মিত সেরা স্বল্পদৈর্ঘ্য চলচ্চিত্রের জন্য নতুন একটি পুরস্কার দেওয়া হবে।', '2026-10-23T23:24:00', 1390, 0, N'images/articles/22-bn-entertainment.jpg', 8, 3, 3, 'bn', '2026-10-23T21:24:00', NULL, 1, '2026-10-23T23:24:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (23, N'ঝড়ের আগাম সতর্কতায় উপকূলীয় দেশগুলোর যৌথ উদ্যোগ', NULL, N'ঝড়ের তথ্য ভাগাভাগি, বন্দরে অভিন্ন সতর্কসংকেত এবং আগাম সতর্কতা কেন্দ্র স্থাপনে একমত হয়েছে কয়েকটি উপকূলীয় দেশ।', N'তীব্র ঝড় সম্পর্কে জাহাজ ও উপকূলবাসীকে আগেভাগে সতর্ক করতে যৌথ পরিকল্পনায় একমত হয়েছে একদল উপকূলীয় দেশ।

আবহাওয়া সংস্থাগুলো তাৎক্ষণিক তথ্য ভাগাভাগি করবে এবং বন্দরগুলো একই সতর্কসংকেত ব্যবহার করবে।

একটি যৌথ তহবিল থেকে বয়া, উপকূলীয় রাডার ও প্রশিক্ষণের খরচ মেটানো হবে।', '2026-10-23T06:36:00', 930, 0, N'images/articles/23-bn-world.jpg', 2, 2, 3, 'bn', '2026-10-23T04:36:00', NULL, 1, '2026-10-23T06:36:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (24, N'গ্রামের পরিবারের জন্য সাশ্রয়ী পানি পরীক্ষার কিট উদ্ভাবন', NULL, N'একদল গবেষক এমন একটি কিট তৈরি করেছেন, যা দিয়ে পরিবারগুলো কয়েক মিনিটে খাবার পানির সাধারণ দূষণ পরীক্ষা করতে পারবে।', N'বিশ্ববিদ্যালয়ের একদল গবেষক কম খরচের একটি কিট তৈরি করেছেন, যা দিয়ে কয়েক মিনিটে খাবার পানি পরীক্ষা করা যায়।

কিটে রং বদলানো স্ট্রিপ ও একটি ছাপানো চার্ট আছে, তাই বিদ্যুৎ বা বিশেষ প্রশিক্ষণ লাগে না।

কয়েকটি গ্রামে মাঠপর্যায়ের পরীক্ষায় বেশির ভাগ ক্ষেত্রে ফলাফল ল্যাবরেটরির পরীক্ষার সঙ্গে মিলেছে।

গবেষকেরা বলছেন, এটি প্রাথমিক পরীক্ষা। দূষণ ধরা পড়লে ল্যাবরেটরিতে নিশ্চিত হওয়া উচিত।', '2026-10-21T09:00:00', 760, 0, N'images/articles/24-bn-science.jpg', 11, 3, 3, 'bn', '2026-10-21T07:00:00', NULL, 1, '2026-10-21T09:00:00', NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (25, N'বাংলা ভাষার এআই সহকারী তৈরিতে কাজ করছেন তরুণ প্রকৌশলীরা', NULL, N'বাংলায় প্রশ্নের উত্তর দিতে পারে এমন এআই সহকারী তৈরিতে কাজ করছে একদল তরুণ প্রকৌশলী।', N'বাংলা ভাষায় প্রশ্ন বুঝে উত্তর দিতে পারে এমন এআই সহকারী তৈরিতে কাজ করছেন একদল তরুণ প্রকৌশলী।

তাঁরা বলছেন, আঞ্চলিক শব্দ ও উচ্চারণের বৈচিত্র্য সবচেয়ে বড় চ্যালেঞ্জ।', '2026-10-25T07:04:48', 0, 0, N'images/articles/25-bn-technology.jpg', 5, 2, 2, 'bn', '2026-10-25T05:04:48', NULL, NULL, NULL, NULL);
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote]) VALUES (26, N'ডেঙ্গু প্রতিরোধে বাড়ির আশপাশে পানি জমতে না দেওয়ার পরামর্শ', NULL, N'বর্ষা মৌসুমে ডেঙ্গুর ঝুঁকি কমাতে বাড়ির আশপাশে জমে থাকা পানি নিয়মিত পরিষ্কারের পরামর্শ দিয়েছেন স্বাস্থ্যকর্মীরা।', N'বর্ষা মৌসুমে ডেঙ্গুর ঝুঁকি কমাতে ফুলের টব, পরিত্যক্ত পাত্র ও ছাদে জমে থাকা পানি নিয়মিত পরিষ্কারের পরামর্শ দিয়েছেন স্বাস্থ্যকর্মীরা।

জ্বর হলে দেরি না করে চিকিৎসকের পরামর্শ নেওয়ার কথাও বলেছেন তাঁরা।', '2026-10-25T07:48:00', 0, 0, N'images/articles/26-bn-health.jpg', 9, 3, 1, 'bn', '2026-10-25T05:48:00', NULL, NULL, NULL, NULL);
SET IDENTITY_INSERT [Articles] OFF;

INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (1, 1);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (1, 18);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (2, 2);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (2, 8);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (3, 20);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (3, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (4, 6);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (4, 7);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (5, 10);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (6, 9);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (7, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (7, 19);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (8, 18);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (8, 19);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (9, 16);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (9, 6);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (10, 17);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (11, 13);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (12, 13);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (13, 4);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (13, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (14, 14);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (14, 15);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (15, 1);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (15, 18);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (16, 2);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (16, 8);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (17, 20);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (17, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (18, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (19, 6);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (19, 7);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (20, 10);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (21, 9);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (22, 12);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (23, 19);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (23, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (24, 15);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (25, 16);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (26, 13);

INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (1, 4, N'Later trains make a real difference for anyone who works late. Hope it becomes permanent.', '2026-10-25T07:24:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (1, 5, N'Would be great to see the same on weekends.', '2026-10-25T08:24:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (5, 6, N'Good to see new players get a chance before the big tournaments.', '2026-10-25T03:24:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (7, 4, N'Shared warning levels across ports is a sensible idea.', '2026-10-24T21:12:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (9, 5, N'Code review is still the step that matters most in my team too.', '2026-10-24T17:24:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (13, 6, N'Our school has power cuts every week. Solar would help a lot.', '2026-10-24T08:48:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (15, 4, N'সন্ধ্যার ট্রেন চালু থাকায় অফিস থেকে ফেরা অনেক সহজ হয়েছে।', '2026-10-25T09:00:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (15, 6, N'সাপ্তাহিক ছুটির দিনেও এই সময়সূচি চাই।', '2026-10-25T09:00:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (20, 5, N'নতুনদের সুযোগ দেওয়া ভালো সিদ্ধান্ত। শুভকামনা রইল।', '2026-10-25T07:00:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (22, 4, N'মোবাইলে বানানো ছবির জন্য পুরস্কারের ধারণাটা দারুণ।', '2026-10-24T10:24:00');
INSERT INTO [Comments] ([ArticleId], [UserId], [Text], [CreatedDate]) VALUES (17, 6, N'আমাদের এলাকাতেও এ বছর বৃষ্টি খুব অনিয়মিত। কৃষকদের জন্য ভালো খবর।', '2026-10-23T04:12:00');

UPDATE [Users] SET [AvatarPath] = CASE [UserId] WHEN 1 THEN N'/images/avatars/editorial-desk.png' WHEN 2 THEN N'/images/avatars/farhana-rahman.png' WHEN 3 THEN N'/images/avatars/tanvir-ahmed.png' WHEN 4 THEN N'/images/avatars/nusrat-jahan.png' WHEN 5 THEN N'/images/avatars/imran-hossain.png' WHEN 6 THEN N'/images/avatars/sadia-islam.png' END;
UPDATE [Users] SET [Bio] = N'The Meridian Newsfolio editorial desk reviews every story before it is published and keeps the archive accurate.' WHERE [UserId] = 1;
UPDATE [Users] SET [Bio] = N'Farhana Rahman covers Dhaka, transport and sport, with a focus on how city life is changing.' WHERE [UserId] = 2;
UPDATE [Users] SET [Bio] = N'Tanvir Ahmed reports on business, technology and science from Chattogram and Dhaka.' WHERE [UserId] = 3;
UPDATE [Users] SET [Bio] = N'Nusrat Jahan is a teacher in Sylhet who reads the archive for stories on education and health.' WHERE [UserId] = 4;
UPDATE [Users] SET [Bio] = N'Imran Hossain follows cricket, football and the local startup scene from Rajshahi.' WHERE [UserId] = 5;
UPDATE [Users] SET [Bio] = N'Sadia Islam is a university student in Khulna interested in climate, science and culture.' WHERE [UserId] = 6;
UPDATE [Articles] SET [Slug] = N'dhaka-commuters-welcome-longer-evening-hours-on-city-rail-services' WHERE [ArticleId] = 1;
UPDATE [Articles] SET [Slug] = N'chattogram-port-speeds-up-container-checks-with-new-digital-booking-system' WHERE [ArticleId] = 2;
UPDATE [Articles] SET [Slug] = N'farmers-in-the-north-turn-to-drought-tolerant-rice-as-dry-spells-grow-longer' WHERE [ArticleId] = 3;
UPDATE [Articles] SET [Slug] = N'bangladeshi-fintech-startups-attract-new-funding-for-rural-digital-payments' WHERE [ArticleId] = 4;
UPDATE [Articles] SET [Slug] = N'bangladesh-name-three-uncapped-players-in-squad-for-tri-series' WHERE [ArticleId] = 5;
UPDATE [Articles] SET [Slug] = N'universities-expand-evening-degree-programmes-for-working-students' WHERE [ArticleId] = 6;
UPDATE [Articles] SET [Slug] = N'coastal-nations-agree-joint-early-warning-plan-for-extreme-storms' WHERE [ArticleId] = 7;
UPDATE [Articles] SET [Slug] = N'new-cross-border-rail-freight-link-halves-journey-times' WHERE [ArticleId] = 8;
UPDATE [Articles] SET [Slug] = N'software-teams-adopt-ai-coding-assistants-but-keep-humans-in-charge-of-review' WHERE [ArticleId] = 9;
UPDATE [Articles] SET [Slug] = N'banks-urge-customers-to-enable-two-step-verification-after-phishing-wave' WHERE [ArticleId] = 10;
UPDATE [Articles] SET [Slug] = N'clinics-extend-evening-hours-for-seasonal-vaccination-campaign' WHERE [ArticleId] = 11;
UPDATE [Articles] SET [Slug] = N'walking-every-day-linked-to-better-sleep-readers-say' WHERE [ArticleId] = 12;
UPDATE [Articles] SET [Slug] = N'rooftop-solar-on-public-schools-cuts-electricity-bills-by-a-third' WHERE [ArticleId] = 13;
UPDATE [Articles] SET [Slug] = N'student-built-satellite-sends-its-first-images-from-orbit' WHERE [ArticleId] = 14;
UPDATE [Articles] SET [Slug] = N'ঢাকায়-সন্ধ্যার-রেলসেবা-বাড়ানোয়-স্বস্তিতে-যাত্রীরা' WHERE [ArticleId] = 15;
UPDATE [Articles] SET [Slug] = N'চট্টগ্রাম-বন্দরে-অনলাইন-বুকিংয়ে-কনটেইনার-পরীক্ষায়-গতি' WHERE [ArticleId] = 16;
UPDATE [Articles] SET [Slug] = N'খরাসহিষ্ণু-ধানে-ঝুঁকছেন-উত্তরাঞ্চলের-কৃষকেরা' WHERE [ArticleId] = 17;
UPDATE [Articles] SET [Slug] = N'উপকূল-রক্ষায়-ম্যানগ্রোভের-চারা-রোপণে-স্বেচ্ছাসেবকেরা' WHERE [ArticleId] = 18;
UPDATE [Articles] SET [Slug] = N'গ্রামের-দোকানে-ডিজিটাল-লেনদেনে-বিনিয়োগ-পাচ্ছে-স্টার্টআপ' WHERE [ArticleId] = 19;
UPDATE [Articles] SET [Slug] = N'ত্রিদেশীয়-সিরিজের-দলে-তিন-নতুন-মুখ' WHERE [ArticleId] = 20;
UPDATE [Articles] SET [Slug] = N'কর্মজীবী-শিক্ষার্থীদের-জন্য-সান্ধ্য-কোর্স-বাড়াচ্ছে-বিশ্ববিদ্যালয়' WHERE [ArticleId] = 21;
UPDATE [Articles] SET [Slug] = N'রেকর্ডসংখ্যক-ছবি-নিয়ে-শুরু-হলো-স্বাধীন-চলচ্চিত্র-উৎসব' WHERE [ArticleId] = 22;
UPDATE [Articles] SET [Slug] = N'ঝড়ের-আগাম-সতর্কতায়-উপকূলীয়-দেশগুলোর-যৌথ-উদ্যোগ' WHERE [ArticleId] = 23;
UPDATE [Articles] SET [Slug] = N'গ্রামের-পরিবারের-জন্য-সাশ্রয়ী-পানি-পরীক্ষার-কিট-উদ্ভাবন' WHERE [ArticleId] = 24;
UPDATE [Articles] SET [Slug] = N'বাংলা-ভাষার-এআই-সহকারী-তৈরিতে-কাজ-করছেন-তরুণ-প্রকৌশলীরা' WHERE [ArticleId] = 25;
UPDATE [Articles] SET [Slug] = N'ডেঙ্গু-প্রতিরোধে-বাড়ির-আশপাশে-পানি-জমতে-না-দেওয়ার-পরামর্শ' WHERE [ArticleId] = 26;
UPDATE [Articles] SET [Slug] = N'coastal-districts-plan-a-mangrove-belt-to-soften-storm-surges' WHERE [ArticleId] = 27;
UPDATE [Articles] SET [Slug] = N'শিক্ষার্থীদের-তৈরি-আবহাওয়া-বেলুন-পাঠাবে-উচ্চ-বায়ুমণ্ডলের-তথ্য' WHERE [ArticleId] = 28;
UPDATE [Tags] SET [Slug] = N'dhaka' WHERE [TagId] = 1;
UPDATE [Tags] SET [Slug] = N'chattogram' WHERE [TagId] = 2;
UPDATE [Tags] SET [Slug] = N'climate' WHERE [TagId] = 3;
UPDATE [Tags] SET [Slug] = N'renewable-energy' WHERE [TagId] = 4;
UPDATE [Tags] SET [Slug] = N'elections' WHERE [TagId] = 5;
UPDATE [Tags] SET [Slug] = N'startups' WHERE [TagId] = 6;
UPDATE [Tags] SET [Slug] = N'markets' WHERE [TagId] = 7;
UPDATE [Tags] SET [Slug] = N'exports' WHERE [TagId] = 8;
UPDATE [Tags] SET [Slug] = N'universities' WHERE [TagId] = 9;
UPDATE [Tags] SET [Slug] = N'cricket' WHERE [TagId] = 10;
UPDATE [Tags] SET [Slug] = N'football' WHERE [TagId] = 11;
UPDATE [Tags] SET [Slug] = N'film' WHERE [TagId] = 12;
UPDATE [Tags] SET [Slug] = N'public-health' WHERE [TagId] = 13;
UPDATE [Tags] SET [Slug] = N'space' WHERE [TagId] = 14;
UPDATE [Tags] SET [Slug] = N'research' WHERE [TagId] = 15;
UPDATE [Tags] SET [Slug] = N'artificial-intelligence' WHERE [TagId] = 16;
UPDATE [Tags] SET [Slug] = N'cybersecurity' WHERE [TagId] = 17;
UPDATE [Tags] SET [Slug] = N'transport' WHERE [TagId] = 18;
UPDATE [Tags] SET [Slug] = N'diplomacy' WHERE [TagId] = 19;
UPDATE [Tags] SET [Slug] = N'agriculture' WHERE [TagId] = 20;
UPDATE [Articles] SET [UpdatedAt] = DATEADD(MINUTE, 35 + [ArticleId] * 7, [PublishDate]) WHERE [StatusId] = 3;
UPDATE [Articles] SET [UpdatedAt] = DATEADD(HOUR, 3, [CreatedAt]) WHERE [StatusId] <> 3;
UPDATE [Articles] SET [ScheduledAt] = [PublishDate] WHERE [StatusId] = 3;
UPDATE [Articles] SET [ScheduledAt] = DATEADD(DAY, 2, [CreatedAt]) WHERE [StatusId] IN (1, 2, 4);
UPDATE [Articles] SET [ReviewNote] = CASE [LanguageCode] WHEN 'bn' THEN N'সূত্র ও তথ্য যাচাইয়ের পর সম্পাদকীয় ডেস্ক অনুমোদন দিয়েছে।' ELSE N'Approved by the editorial desk after checking sources and figures.' END WHERE [StatusId] = 3;
UPDATE [Articles] SET [ReviewedById] = 1, [ReviewedAt] = DATEADD(HOUR, 1, [CreatedAt]),
    [ReviewNote] = CASE [LanguageCode] WHEN 'bn' THEN N'আগের খসড়ায় একটি উৎস যোগ করতে বলা হয়েছিল; লেখক তা যোগ করে আবার জমা দিয়েছেন।' ELSE N'An earlier version was returned to add a second source; the author has resubmitted it.' END
WHERE [StatusId] IN (1, 2);
SET IDENTITY_INSERT [Articles] ON;
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote], [ScheduledAt]) VALUES (27, N'Coastal districts plan a mangrove belt to soften storm surges', N'coastal-districts-plan-a-mangrove-belt-to-soften-storm-surges', N'Officials in three coastal districts want to plant a continuous belt of mangroves along embankments, saying the trees can reduce the force of storm surges and protect farmland.', N'Officials in three coastal districts have proposed planting a continuous belt of mangroves along vulnerable embankments before next year''s cyclone season.

## Why mangroves

Mangrove roots slow down waves and trap sediment, which helps embankments last longer. Local engineers say a wide belt of trees can reduce the height of a storm surge before it reaches villages.

> "We cannot stop storms, but we can make them less destructive," said one district planner.

## What happens next

- Nurseries will raise seedlings over the winter.
- Planting will start along the most exposed stretches first.
- Communities will be paid to look after the young trees for three years.

The plan still needs final approval and funding from the national budget.', DATEADD(DAY, 3, CAST(CAST(GETDATE() AS date) AS datetime)) + CAST('08:00' AS datetime), 0, 0, N'images/articles/27-en-environment.jpg', 10, 2, 5, 'en', DATEADD(DAY, -1, GETDATE()), DATEADD(HOUR, -6, GETDATE()), 1, DATEADD(HOUR, -5, GETDATE()), N'Approved and scheduled for the start of the cyclone preparedness week.', DATEADD(DAY, 3, CAST(CAST(GETDATE() AS date) AS datetime)) + CAST('08:00' AS datetime));
INSERT INTO [Articles] ([ArticleId], [Title], [Slug], [Summary], [Content], [PublishDate], [ViewsCount], [IsBreakingNews], [ImagePath], [CategoryId], [AuthorId], [StatusId], [LanguageCode], [CreatedAt], [UpdatedAt], [ReviewedById], [ReviewedAt], [ReviewNote], [ScheduledAt]) VALUES (28, N'শিক্ষার্থীদের তৈরি আবহাওয়া বেলুন পাঠাবে উচ্চ বায়ুমণ্ডলের তথ্য', N'শিক্ষার্থীদের-তৈরি-আবহাওয়া-বেলুন-পাঠাবে-উচ্চ-বায়ুমণ্ডলের-তথ্য', N'একটি বিশ্ববিদ্যালয়ের শিক্ষার্থীরা নিজেরাই আবহাওয়া বেলুন ও সেন্সর তৈরি করেছেন। বেলুনটি প্রায় ত্রিশ কিলোমিটার উঁচু থেকে তাপমাত্রা ও বাতাসের তথ্য পাঠাবে।', N'একটি বিশ্ববিদ্যালয়ের শিক্ষার্থীরা নিজেরাই আবহাওয়া বেলুন ও তার সেন্সর তৈরি করেছেন। আগামী সপ্তাহে বেলুনটি উড়ানো হবে।

## কী মাপা হবে

বেলুনটি প্রায় ত্রিশ কিলোমিটার উঁচুতে উঠে **তাপমাত্রা**, **বায়ুচাপ** ও **বাতাসের গতি** মাপবে এবং রেডিওর মাধ্যমে তথ্য পাঠাবে।

> "নিজেদের হাতে তৈরি যন্ত্র আকাশে পাঠানো আমাদের জন্য বড় অভিজ্ঞতা," বললেন দলের এক সদস্য।

## কেন গুরুত্বপূর্ণ

- স্থানীয় আবহাওয়ার পূর্বাভাস আরও নির্ভুল করতে সাহায্য করবে।
- সংগ্রহ করা তথ্য অন্য গবেষকদের জন্য উন্মুক্ত রাখা হবে।
- স্কুলের শিক্ষার্থীদের বিজ্ঞানে আগ্রহী করতে প্রদর্শনী করা হবে।', DATEADD(DAY, 3, CAST(CAST(GETDATE() AS date) AS datetime)) + CAST('08:00' AS datetime), 0, 0, N'images/articles/28-bn-science.jpg', 11, 3, 5, 'bn', DATEADD(DAY, -1, GETDATE()), DATEADD(HOUR, -6, GETDATE()), 1, DATEADD(HOUR, -5, GETDATE()), N'অনুমোদিত। উৎক্ষেপণের দিন সকালে প্রকাশের জন্য নির্ধারিত।', DATEADD(DAY, 3, CAST(CAST(GETDATE() AS date) AS datetime)) + CAST('08:00' AS datetime));
SET IDENTITY_INSERT [Articles] OFF;
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (27, 3);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (27, 4);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (27, 20);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (28, 14);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (28, 15);
INSERT INTO [ContentLabels] ([ArticleId], [TagId]) VALUES (28, 9);
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (4, 6, DATEADD(MINUTE, -47, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (4, 11, DATEADD(MINUTE, -94, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (4, 21, DATEADD(MINUTE, -141, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (4, 23, DATEADD(MINUTE, -188, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (5, 5, DATEADD(MINUTE, -235, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (5, 20, DATEADD(MINUTE, -282, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (5, 9, DATEADD(MINUTE, -329, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (5, 19, DATEADD(MINUTE, -376, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (6, 13, DATEADD(MINUTE, -423, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (6, 14, DATEADD(MINUTE, -470, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (6, 24, DATEADD(MINUTE, -517, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (6, 22, DATEADD(MINUTE, -564, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (2, 4, DATEADD(MINUTE, -611, GETDATE()));
INSERT INTO [Bookmarks] ([UserId], [ArticleId], [CreatedAt]) VALUES (3, 7, DATEADD(MINUTE, -658, GETDATE()));
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 6, DATEADD(MINUTE, -2972, GETDATE()), DATEADD(MINUTE, -92, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 11, DATEADD(MINUTE, -3067, GETDATE()), DATEADD(MINUTE, -187, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 21, DATEADD(MINUTE, -3162, GETDATE()), DATEADD(MINUTE, -282, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 23, DATEADD(MINUTE, -6137, GETDATE()), DATEADD(MINUTE, -377, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 13, DATEADD(MINUTE, -6232, GETDATE()), DATEADD(MINUTE, -472, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 24, DATEADD(MINUTE, -7767, GETDATE()), DATEADD(MINUTE, -567, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 1, DATEADD(MINUTE, -3542, GETDATE()), DATEADD(MINUTE, -662, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (4, 15, DATEADD(MINUTE, -2197, GETDATE()), DATEADD(MINUTE, -757, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 5, DATEADD(MINUTE, -1545, GETDATE()), DATEADD(MINUTE, -105, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 20, DATEADD(MINUTE, -1640, GETDATE()), DATEADD(MINUTE, -200, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 9, DATEADD(MINUTE, -7495, GETDATE()), DATEADD(MINUTE, -295, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 19, DATEADD(MINUTE, -7590, GETDATE()), DATEADD(MINUTE, -390, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 4, DATEADD(MINUTE, -7685, GETDATE()), DATEADD(MINUTE, -485, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 22, DATEADD(MINUTE, -4900, GETDATE()), DATEADD(MINUTE, -580, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (5, 2, DATEADD(MINUTE, -4995, GETDATE()), DATEADD(MINUTE, -675, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 13, DATEADD(MINUTE, -5878, GETDATE()), DATEADD(MINUTE, -118, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 14, DATEADD(MINUTE, -7413, GETDATE()), DATEADD(MINUTE, -213, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 24, DATEADD(MINUTE, -7508, GETDATE()), DATEADD(MINUTE, -308, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 22, DATEADD(MINUTE, -4723, GETDATE()), DATEADD(MINUTE, -403, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 18, DATEADD(MINUTE, -6258, GETDATE()), DATEADD(MINUTE, -498, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 7, DATEADD(MINUTE, -4913, GETDATE()), DATEADD(MINUTE, -593, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (6, 3, DATEADD(MINUTE, -6448, GETDATE()), DATEADD(MINUTE, -688, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (2, 4, DATEADD(MINUTE, -7266, GETDATE()), DATEADD(MINUTE, -66, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (2, 9, DATEADD(MINUTE, -7361, GETDATE()), DATEADD(MINUTE, -161, GETDATE()), 4);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (2, 16, DATEADD(MINUTE, -3136, GETDATE()), DATEADD(MINUTE, -256, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (3, 7, DATEADD(MINUTE, -4399, GETDATE()), DATEADD(MINUTE, -79, GETDATE()), 3);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (3, 1, DATEADD(MINUTE, -3054, GETDATE()), DATEADD(MINUTE, -174, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (3, 17, DATEADD(MINUTE, -4589, GETDATE()), DATEADD(MINUTE, -269, GETDATE()), 1);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (1, 8, DATEADD(MINUTE, -5813, GETDATE()), DATEADD(MINUTE, -53, GETDATE()), 2);
INSERT INTO [ReadingHistory] ([UserId], [ArticleId], [FirstReadAt], [LastReadAt], [ReadCount]) VALUES (1, 11, DATEADD(MINUTE, -3028, GETDATE()), DATEADD(MINUTE, -148, GETDATE()), 1);
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (1, 'C25353171A843C3F580E6D16C78572AB8B98480E39B2AB290B994AFB305CC352', 'web', DATEADD(HOUR, -30, GETDATE()), DATEADD(HOUR, -18, GETDATE()), DATEADD(HOUR, -20, GETDATE()));
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (2, 'CFAA2EFB72B4A7AAD3489652569FD20C5A2747AC3DA0CB6312D61DF5F1C28E2C', 'api', DATEADD(HOUR, -31, GETDATE()), DATEADD(HOUR, -19, GETDATE()), DATEADD(HOUR, -21, GETDATE()));
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (3, '4DDE73A5E0E4F79DEE7A66E4B955A6AC5B58AD038B530B1304C9B51CD14D80E7', 'web', DATEADD(HOUR, -32, GETDATE()), DATEADD(HOUR, -20, GETDATE()), DATEADD(HOUR, -22, GETDATE()));
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (4, 'FD10C8122997A12736419578F009DD1B7057B27D5FDEBB876D9647C0100EE0E0', 'api', DATEADD(HOUR, -33, GETDATE()), DATEADD(HOUR, -21, GETDATE()), DATEADD(HOUR, -23, GETDATE()));
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (5, 'E34B02906B26A73126C771360570E8C05E4CD0A7F4B49FE32DB5EB5D0D4CA9EB', 'web', DATEADD(HOUR, -34, GETDATE()), DATEADD(HOUR, -22, GETDATE()), DATEADD(HOUR, -24, GETDATE()));
INSERT INTO [UserTokens] ([UserId], [TokenHash], [ClientType], [CreatedAt], [ExpiresAt], [RevokedAt]) VALUES (6, '3B5188D7386D8AB76AEE179C9DAC8656DC45E032E86B14AD552BCEED2822D037', 'api', DATEADD(HOUR, -35, GETDATE()), DATEADD(HOUR, -23, GETDATE()), DATEADD(HOUR, -25, GETDATE()));

COMMIT TRANSACTION;
