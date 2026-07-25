# Website & Docs

The [getlibation.com](https://getlibation.com) site is built with [VitePress](https://vitepress.dev/). Site content lives in this repo: the home page is `index.md` at the root, documentation pages are under `docs/`, and site config / theme files are in `.vitepress/`.

For VitePress details (markdown, routing, and more), see the [VitePress guide](https://vitepress.dev/guide).

## Prerequisites

- [Node.js](https://nodejs.org/) 18 or newer

## Run the site locally

From the **repository root** (`Libation/`, where `package.json` is):

### One-time setup

Install npm dependencies (needed once after cloning, or again after dependency changes):

```bash
npm install
```

### Start the dev server

```bash
npm run docs:dev
```

Then open `http://localhost:5173` in your browser. The server reloads when you edit markdown or theme files.

Stop it with `Ctrl+C` in that terminal.

### Other commands

```bash
# Production build (output: .vitepress/dist)
npm run docs:build

# Preview the production build locally
npm run docs:preview
```

## Adding pages

New markdown files are routed from their path automatically (for example `docs/getting-started.md` -> `/docs/getting-started`, or `donate.md` at the repo root -> `/donate`).

To show a page in the sidebar or top nav, add it in `.vitepress/config.js`. A page can exist and be reachable by URL without being linked in navigation.
