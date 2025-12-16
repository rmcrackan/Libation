import { defineConfig } from "vitepress";

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Libation",
  description: "Libation: Liberate your Library - A free application for downloading your Audible audiobooks",
  head: [["link", { rel: "icon", href: "/favicon.ico" }]],
  cleanUrls: true,
  rewrites: {
    "docs/index.md": "docs/getting-started.md",
  },
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
      pattern: "https://github.com/rmcrackan/Libation/edit/main/:path",
    },

    lastUpdated: true,

    nav: [
      { text: "Getting Started", link: "/docs/getting-started" },
      { text: "Download", link: "https://github.com/rmcrackan/Libation/releases/latest" },
      { text: "Report Issues", link: "https://github.com/rmcrackan/Libation/issues" },
      { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
    ],
    sidebar: [
      {
        items: [
          { text: "Getting Started", link: "/docs/index" },
          { text: "FAQ", link: "/docs/frequently-asked-questions" },
          {
            text: "Report Issues",
            link: "https://github.com/rmcrackan/Libation/issues",
          },
          { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
        ],
      },
      {
        text: "Installation",
        collapsed: false,

        items: [
          {
            text: "Install on Linux",
            link: "/docs/installation/linux",
          },
          { text: "Install on Mac", link: "/docs/installation/mac" },
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
        ],
      },
      {
        text: "Advanced",
        collapsed: false,
        items: [
          { text: "Advanced Topics", link: "/docs/advanced/advanced" },
          {
            text: "Linux Development Setup",
            link: "/docs/advanced/linux-development-setup-using-nix",
          },
        ],
      },
    ],

    outline: {
      level: "deep",
    },

    socialLinks: [{ icon: "github", link: "https://github.com/rmcrackan/Libation" }],

    search: {
      provider: "local",
    },
  },
});
