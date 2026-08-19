# Retrying titles Audible refuses

Some titles in your library cannot be downloaded right now. Audible refuses a content license for a title
you no longer own, for a Plus title that has left the catalog, or for an account that is no longer active.
Others fail because Audible has no audio to send yet, which is what a preorder looks like before its release
date.

Libation remembers these refusals and waits before asking again. There is nothing to turn on and nothing to
configure.

## Why it matters

Without this, a title Audible had just refused was requested again on the very next run. For a scheduled
`libationcli liberate` — a cron job or the Docker image's own loop — that meant re-requesting the same
refused licenses every run, forever: pointless traffic to Audible, which itself risks throttling, and a
console and log full of the same warning for the same titles.

## How long Libation waits

The wait starts short and doubles with each refusal in a row. Nothing is ever permanent: every kind of
failure is attempted again on its own, because Audible never tells us the difference between "you will never
have rights to this" and "not right now".

| What happened | First wait | Longest wait |
|---------------|-----------|--------------|
| Audible refused a license and named an eligibility reason: not owned, not in the Plus catalog, account not entitled | 1 day | 30 days |
| Audible has no downloadable audio for the title, as for an unreleased preorder | 6 hours | 7 days |
| Audible refused but would not say why, which usually means an outage or throttling | 1 hour | 12 hours |

A refusal for a *different* reason than last time starts the count over: Audible changed its mind about why,
so the wait built up for the old reason no longer describes the situation.

Failures that are nothing to do with Audible — a dropped connection, a decrypt error, a full disk — are not
waited on at all. They keep being retried on the next run, because nothing about them suggests the next
attempt fails the same way.

## Asking for a title anyway

Any of these overrides the wait, and clears it so the schedule starts from the beginning if the attempt fails
again:

- `libationcli liberate <ASIN>` — naming a title always attempts it.
- `libationcli liberate --force` — attempts everything, including the refused titles.
- In the app, selecting a single title and downloading it.
- Setting a title's download status to **Not Downloaded** (grid context menu, book details, or
  `libationcli set-status`).
- A successful download, which forgets the title's history entirely.

## What you see

**On the command line**, one summary per run instead of a warning block per title:

```
Skipped 4 titles that recently failed to download. Libation will try again by itself.
  Audible denied a download license: 3 (next attempt in about 20 hours)
  Audible has no downloadable audio yet: 1 (next attempt in about 2 hours)
  To try one now: libationcli liberate <ASIN>. For all of them: libationcli liberate --force.
```

The run that first hits a refusal still prints Audible's full explanation for that title, and then says when
the title will be attempted again, so a schedule that goes quiet about a title explains itself rather than
appearing to have forgotten it.

**In the app**, a multi-title download leaves waited-on titles out of the queue and reports them under
"Waiting before trying again after a recent failure", with what Audible said and when each comes back. A
single-title download is never held back.

## PDFs are waited on too

Libation fetches a PDF through the same license request as the audiobook, so a title Audible has refused would
be refused again for its PDF. Everything above therefore applies to a title whose PDF is all that is missing,
and to a `libationcli liberate --pdf` run.

When a run downloads both halves of a title, the PDF comes from the license the audiobook download already
has, so a title costs one license request however much of it Libation is fetching.

## Titles absent from your last library scan

Separately from any wait, a bulk run leaves alone the titles your last scan did not find — a returned title, or
one that has left the Plus catalog. Audible will not license a title it no longer lists for the account, so
attempting one collects a refusal and downloads nothing. The app has always worked this way; the command line
now does too:

```
Skipped 12 titles absent from your last library scan. Audible will not license a title it no longer lists, so
run Scan, or `libationcli scan`, then try again. To attempt them anyway: libationcli liberate --force.
```

Run a scan first if the titles should still be in your library. `libationcli liberate --force`, and naming a
title, attempt them regardless.

## When Audible has no PDF at all

Some titles are listed as having a PDF that Audible will not deliver: the license comes back granted, with no
link in it. Libation marks that title's PDF as an error and stops asking, because nothing about asking again
would change the answer. The grid says "PDF could not be downloaded and will not be tried again", and setting
the PDF status back to **Not Downloaded** tries once more.

## Relationship to marking a book as an error

This is separate from the app's Abort / Retry / **Ignore** prompt. Choosing Ignore sets a title's download
status to Error, which stops Libation attempting it until you change the status back yourself. That is a
decision you make; the wait described here is automatic, temporary, and needs nothing from you. The PDF of a
title Audible has none for is written off the same way, and is likewise yours to reset.

## Where it is stored

In Libation's database, alongside the record backing the [daily download
limit](/docs/features/daily-download-limit). The database rather than a file in the Libation Files directory,
because in Docker that directory lives inside the container and only the database is on a volume — a
file-based record would forget every refusal on each container start, which is exactly the case this fixes.
