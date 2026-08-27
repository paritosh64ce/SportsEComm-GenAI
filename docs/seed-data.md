# Seed Data

The database is auto-seeded on first startup via `DbInitializer.InitializeAsync`. No manual migrations needed.

---

## Demo customers

All secrets are stored as bcrypt hashes. Use the plaintext values below only for local login.

| # | Name | Email | Secret key | Demo login |
|---|------|-------|-----------|------------|
| 1 | MS Dhoni | `ms.dhoni@teamindia2011.com` | `dhoni7#cup` | ✅ |
| 2 | Sachin Tendulkar | `sachin.tendulkar@teamindia2011.com` | `sachin10#master` | ✅ |
| 3 | Virat Kohli | `virat.kohli@teamindia2011.com` | `kohli18#chase` | ✅ |
| 4 | Yuvraj Singh | `yuvraj.singh@teamindia2011.com` | `yuvraj12#champ` | ✅ |
| 5 | Gautam Gambhir | `gautam.gambhir@teamindia2011.com` | `squad2011` | — |
| 6 | Virender Sehwag | `virender.sehwag@teamindia2011.com` | `squad2011` | — |
| 7 | Zaheer Khan | `zaheer.khan@teamindia2011.com` | `squad2011` | — |
| 8 | Harbhajan Singh | `harbhajan.singh@teamindia2011.com` | `squad2011` | — |
| 9 | Suresh Raina | `suresh.raina@teamindia2011.com` | `squad2011` | — |
| 10 | Munaf Patel | `munaf.patel@teamindia2011.com` | `squad2011` | — |
| 11 | S Sreesanth | `s.sreesanth@teamindia2011.com` | `squad2011` | — |

> **Demo login** means `IsDemoLoginEnabled = true`. The chatbot `/login` command works for these four accounts.

---

## Product catalog

| ID | Product | Price (₹) | Stock |
|----|---------|-----------|-------|
| 1 | MSD Signature English Willow Cricket Bat | 28,999 | 15 |
| 2 | Master Blaster Pro Cricket Bat | 19,999 | 20 |
| 3 | World Cup Edition Leather Cricket Ball (Pack of 6) | 4,500 | 100 |
| 4 | ProKeeper Cricket Wicketkeeping Gloves | 3,499 | 30 |
| 5 | TurboFlex Full Cricket Batting Pads | 6,999 | 25 |
| 6 | StumpVision Pro Cricket Helmet | 8,499 | 18 |
| 7 | All-Rounder Heavy-Duty Cricket Kit Bag | 12,999 | 12 |

---

## Pre-seeded order

MS Dhoni (customer 1) has one pre-seeded order for the MSD Signature bat so that the order history endpoints return data immediately on a fresh database.
