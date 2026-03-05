<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { getDetectOS } from './download-detection.js'

const PREVIEW_OPTIONS = [
  { value: '', label: 'Current system' },
  { value: 'windows|x64', label: 'Windows (x64)' },
  { value: 'windows|arm64', label: 'Windows (arm64)' },
  { value: 'mac|x64', label: 'Mac (x64)' },
  { value: 'mac|arm64', label: 'Mac (arm64)' },
  { value: 'linux|x64|', label: 'Linux (x64)' },
  { value: 'linux|x64|Ubuntu', label: 'Linux Ubuntu (x64)' },
  { value: 'linux|x64|Fedora', label: 'Linux Fedora (x64)' },
  { value: 'linux|arm64|', label: 'Linux (arm64)' },
  { value: 'linux|arm64|Ubuntu', label: 'Linux Ubuntu (arm64)' },
  { value: 'linux|arm64|Fedora', label: 'Linux Fedora (arm64)' },
]

const INSTALL_MAC_URL = 'https://getlibation.com/docs/installation/mac'
const INSTALL_LINUX_URL = 'https://getlibation.com/docs/installation/linux'

const status = ref('loading') // 'loading' | 'ready' | 'error'
const errorMessage = ref('')
const cachedAssets = ref([])
const rec = ref(null)
const blockHead = ref('')
const previewValue = ref('')
const installGuideUrl = ref(null)
const installGuideLabel = ref('')
const d = ref(null)

function getPreviewOverrides() {
  if (!previewValue.value) return null
  const parts = previewValue.value.split('|')
  const overrides = { os: parts[0], architecture: parts[1] }
  if (parts.length > 2 && parts[2]) overrides.linuxFlavor = parts[2]
  return overrides
}

function recommendedForLabel(overrides) {
  if (!d.value) return 'Recommended download for your system'
  const os = (overrides && overrides.os) || d.value.os
  const ver = (overrides && overrides.osVersion) || d.value.osVersion
  const arch = (overrides && overrides.architecture) || d.value.architecture
  const flavor = (overrides && overrides.linuxFlavor !== undefined) ? overrides.linuxFlavor : d.value.linuxFlavor
  if (os === 'windows') return 'Recommended download for Windows (' + arch + ')'
  if (os === 'mac') return 'Recommended download for Mac ' + (ver ? ver + ' ' : '') + '(' + arch + ')'
  if (os === 'linux') return 'Recommended download for ' + (flavor ? flavor + ' ' : '') + 'Linux (' + arch + ')'
  return 'Recommended download for your system'
}

function updateRecommendation() {
  if (!cachedAssets.value.length || !d.value) return
  const overrides = getPreviewOverrides()
  rec.value = d.value.recommendDownload(cachedAssets.value, overrides)
  blockHead.value = recommendedForLabel(overrides)
  const os = (overrides && overrides.os) || d.value.os
  if (os === 'mac') {
    installGuideUrl.value = INSTALL_MAC_URL
    installGuideLabel.value = 'Install on macOS'
  } else if (os === 'linux') {
    installGuideUrl.value = INSTALL_LINUX_URL
    installGuideLabel.value = 'Install on Linux'
  } else {
    installGuideUrl.value = null
    installGuideLabel.value = ''
  }
}

const showReasonWarn = computed(() => {
  return rec.value?.reason && rec.value.reason !== 'Recommended for your system.'
})

const showAssets = computed(() => {
  if (!rec.value || !d.value || status.value !== 'ready') return []
  const libationAssets = cachedAssets.value
  const overrides = getPreviewOverrides()
  const os = (overrides && overrides.os) || d.value.os
  const arch = (overrides && overrides.architecture) || d.value.architecture
  return libationAssets.filter((a) => {
    const p = d.value.parseAssetFilename(a.name)
    if (!p) return false
    if (os === 'windows' || os === 'mac' || os === 'linux') {
      if (p.platform !== os) return false
    }
    if (arch === 'x64' || arch === 'arm64') {
      if (p.arch !== arch) return false
    }
    return true
  })
})

watch(previewValue, () => updateRecommendation())

onMounted(() => {
  d.value = getDetectOS()
  fetch('https://api.github.com/repos/rmcrackan/Libation/releases/latest')
    .then((res) => (res.ok ? res.json() : Promise.reject(new Error('Failed to load release'))))
    .then((release) => {
      cachedAssets.value = (release.assets || []).map((a) => ({
        name: a.name,
        url: a.browser_download_url,
      }))
      status.value = 'ready'
      updateRecommendation()
    })
    .catch((err) => {
      status.value = 'error'
      errorMessage.value = err.message || 'Could not load latest release.'
    })
})
</script>

<template>
  <div class="download-widget-wrapper">
    <div v-if="status === 'loading'" class="download-widget recommended">
      Loading latest release…
    </div>
    <div v-else-if="status === 'error'" class="download-widget recommended">
      <p class="warn">{{ errorMessage }}</p>
    </div>
    <div v-else class="download-widget recommended">
      <div class="block-head">
        <svg class="icon-download" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
          <polyline points="7 10 12 15 17 10" />
          <line x1="12" y1="15" x2="12" y2="3" />
        </svg>
        <span>{{ blockHead }}{{ rec?.windowsX64Choice ? ':' : rec?.recommended ? ':' : rec ? '.' : '' }}</span>
        <select v-model="previewValue" aria-label="Find recommended download for" class="preview-select">
          <option v-for="o in PREVIEW_OPTIONS" :key="o.value || 'current'" :value="o.value">
            {{ o.label }}
          </option>
        </select>
      </div>

      <!-- Windows x64: Chardonnay + Classic -->
      <template v-if="rec?.windowsX64Choice">
        <ul class="widget-list">
          <li><strong>Chardonnay</strong> (modern UI): <a :href="rec.windowsX64Choice.chardonnay.url" download>{{ rec.windowsX64Choice.chardonnay.name }}</a></li>
          <li><strong>Classic</strong> (compact, screenreader-friendly): <a :href="rec.windowsX64Choice.classic.url" download>{{ rec.windowsX64Choice.classic.name }}</a></li>
        </ul>
        <p class="widget-link"><a :href="rec.classicVsChardonnayFaqUrl" target="_blank" rel="noopener">What's the difference between Classic and Chardonnay?</a></p>
        <p v-if="showReasonWarn" class="warn">{{ rec.reason }}</p>
      </template>

      <!-- Single recommended -->
      <template v-else-if="rec?.recommended">
        <p class="widget-download"><a :href="rec.recommended.url" download>{{ rec.recommended.name }}</a></p>
        <p v-if="installGuideUrl" class="widget-link">
          <a :href="installGuideUrl" target="_blank" rel="noopener">{{ installGuideLabel }}</a>
        </p>
        <p v-if="showReasonWarn" class="warn">{{ rec.reason }}</p>
        <div v-if="rec.alternatives?.length" class="alternatives">
          {{ rec.alternatives.length === 1 ? 'Another option:' : 'Other options:' }}
          <ul>
            <li v-for="a in rec.alternatives" :key="a.name">
              <a :href="a.url" download>{{ a.name }}</a>
            </li>
          </ul>
        </div>
      </template>

      <!-- No match -->
      <template v-else-if="rec">
        <p class="warn">{{ rec.reason || 'Could not recommend a download.' }}</p>
        <div class="alternatives">
          Available:
          <ul>
            <li v-for="a in showAssets" :key="a.name">
              <a :href="a.url" download>{{ a.name }}</a>
            </li>
          </ul>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.download-widget-wrapper {
  margin: 1.5rem auto;
  max-width: 42rem;
}
.download-widget.recommended {
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-weight: 600;
}
.download-widget.recommended a {
  color: var(--vp-c-brand-1);
  text-decoration: underline;
}
.download-widget.recommended a:hover {
  text-decoration: underline;
  opacity: 0.8;
}
.block-head {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  margin-bottom: 0.5rem;
}
.block-head .icon-download {
  flex-shrink: 0;
  width: 1.25rem;
  height: 1.25rem;
  color: var(--vp-c-brand-1);
}
.preview-select {
  margin-left: auto;
  font-size: 0.85rem;
  padding-right: 1.75rem;
  padding-left: 0.5rem;
  appearance: none;
  background-color: var(--vp-c-bg-soft);
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'%3E%3Cpath d='M6 9l6 6 6-6'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 0.35rem center;
  border: 1px solid var(--vp-c-divider);
  border-radius: 4px;
  cursor: pointer;
}
.widget-list {
  margin: 0 0 0 1rem;
  padding-left: 0.5rem;
}
.widget-list li strong {
  font-weight: 700;
}
.widget-download {
  margin: 0.25rem 0;
}
.widget-link {
  margin: 0.5rem 0 0;
}
.warn {
  background: var(--vp-custom-block-warning-bg);
  color: var(--vp-custom-block-warning-text);
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  margin: 0.5rem 0 0;
  font-size: 0.9rem;
  font-weight: normal;
}
.alternatives {
  font-size: 0.9rem;
  font-weight: normal;
  color: var(--vp-c-text-2);
  margin-top: 0.5rem;
}
.alternatives ul {
  margin: 0.25rem 0 0 1rem;
  padding-left: 0.5rem;
}
</style>
