import { defineConfig } from "vitepress";

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Libation",
  description:
    "Libation: Liberate your Library - A free application for downloading your Audible audiobooks",
  head: [["link", { rel: "icon", href: "/favicon.ico" }]],
  cleanUrls: true,
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: {
      light: "/libation_logo_light.svg",
      dark: "/libation_logo_dark.svg",
    },

    footer: {
      message: "Released under the GPLv3 License",
    },

    editLink: {
      pattern: "https://github.com/rmcrackan/Libation/edit/master/:path",
    },

    lastUpdated: true,

    nav: [
      { text: "Getting Started", link: "/docs/getting-started" },
      { text: "Docs", link: "/docs/index" },
      {
        text: "Download",
        link: "https://github.com/rmcrackan/Libation/releases/latest",
      },
      {
        text: "Issues & Requests",
        link: "https://github.com/rmcrackan/Libation/issues",
      },
      { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
    ],
    sidebar: [
      {
        items: [
          { text: "Overview", link: "/docs/index" },
          { text: "Getting Started", link: "/docs/getting-started" },
          { text: "FAQ", link: "/docs/frequently-asked-questions" },
          {
            text: "Issues & Requests",
            link: "https://github.com/rmcrackan/Libation/issues",
          },
          { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
        ],
      },
      {
        text: "Installation",
        collapsed: false,
        items: [
          { text: "Linux", link: "/docs/installation/linux" },
          { text: "Mac", link: "/docs/installation/mac" },
          { text: "Docker", link: "/docs/installation/docker" },
        ],
      },
      {
        text: "Features",
        collapsed: false,
        items: [
          {
            text: "Audio File Formats",
            link: "/docs/features/audio-file-formats",
          },
          { text: "Naming Templates", link: "/docs/features/naming-templates" },
          {
            text: "Searching & Filtering",
            link: "/docs/features/searching-and-filtering",
          },
          {
            text: "Easy guide to searching",
            link: "/docs/features/lucene",
          },
        ],
      },
      {
        text: "Advanced",
        collapsed: false,
        items: [
          { text: "Advanced Topics", link: "/docs/advanced/advanced" },
          { text: "Troubleshooting", link: "/docs/advanced/troubleshoot" }
        ],
      },
      {
        text: "Development",
        collapsed: false,
        items: [
          {
            text: "Getting Started",
            link: "/docs/development/getting-started",
          },
          { text: "Contribute", link: "/docs/development/contribute" },
          { text: "Website & Docs", link: "/docs/development/website" },
          { text: "Linux Setup (Nix)", link: "/docs/development/nix-linux-setup" },
        ],
      },
    ],

    outline: {
      level: "deep",
    },

    socialLinks: [
      { icon: "github", link: "https://github.com/rmcrackan/Libation" },
    ],

    search: {
      provider: "local",
    },
  },
});
