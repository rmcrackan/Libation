<script setup>
import { ref, onMounted, defineAsyncComponent } from 'vue'
import DefaultTheme from 'vitepress/theme'
import { useData } from 'vitepress'

const { frontmatter } = useData()
const { Layout: DefaultLayout } = DefaultTheme
const mounted = ref(false)
onMounted(() => { mounted.value = true })

// Load widget only on client so download-detection.js (navigator) never runs during SSR
const DownloadSuggestionWidget = defineAsyncComponent(() =>
  import('./DownloadSuggestionWidget.vue')
)
</script>

<template>
  <DefaultLayout>
    <template #home-hero-after>
      <DownloadSuggestionWidget v-if="frontmatter.layout === 'home' && mounted" />
    </template>
  </DefaultLayout>
</template>
