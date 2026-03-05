/**
 * Client-side OS and system detection for download recommendation.
 * Uses navigator.userAgent and (when available) navigator.userAgentData.
 * Call getDetectOS() in the browser only (e.g. in Vue onMounted).
 */
export function getDetectOS() {
  const ua = typeof navigator !== 'undefined' ? navigator.userAgent : ''
  const uaData = typeof navigator !== 'undefined' && navigator.userAgentData

  function getOS() {
    if (uaData?.platform) {
      const p = uaData.platform.toLowerCase()
      if (p === 'windows') return 'windows'
      if (p === 'macos') return 'mac'
      if (p === 'linux') return 'linux'
      if (p === 'android') return 'android'
      if (p === 'ios') return 'ios'
    }
    if (/\bWindows\b/i.test(ua)) return 'windows'
    if (/\bMac\b/i.test(ua) || /\bMacintosh\b/i.test(ua)) return 'mac'
    if (/\bLinux\b/i.test(ua) && !/\bAndroid\b/i.test(ua)) return 'linux'
    if (/\bAndroid\b/i.test(ua)) return 'android'
    if (/\b(iPhone|iPad|iPod)\b/i.test(ua)) return 'ios'
    return 'other'
  }

  function getOSVersion() {
    const os = getOS()
    if (os === 'windows') {
      const ntMatch = ua.match(/Windows NT (\d+\.\d+)/)
      if (ntMatch) {
        const nt = ntMatch[1]
        if (nt === '10.0') return '10 or 11 (NT 10.0)'
        if (nt === '6.3') return '8.1'
        if (nt === '6.2') return '8'
        if (nt === '6.1') return '7'
        return `NT ${nt}`
      }
      return null
    }
    if (os === 'mac') {
      const macMatch = ua.match(/Mac OS X (\d+[._]\d+(?:[._]\d+)?)/)
      return macMatch ? macMatch[1].replace(/_/g, '.') : null
    }
    if (os === 'linux') {
      const linuxMatch = ua.match(/Linux ([^\s;)]+)/)
      return linuxMatch ? linuxMatch[1] : null
    }
    if (os === 'android') {
      const m = ua.match(/Android (\d+(?:\.\d+)*)/)
      return m ? m[1] : null
    }
    if (os === 'ios') {
      const m = ua.match(/OS (\d+[._]\d+(?:[._]\d+)?)/)
      return m ? m[1].replace(/_/g, '.') : null
    }
    return null
  }

  function getArchitecture() {
    if (uaData?.architecture) {
      const a = uaData.architecture.toLowerCase()
      if (a === 'x86' || a === 'amd64') return a === 'amd64' ? 'x64' : 'x86'
      if (a === 'arm' || a === 'arm64' || a === 'aarch64') return a === 'aarch64' ? 'arm64' : a
      return a
    }
    if (/\b(aarch64|arm64|ARM64)\b/i.test(ua)) return 'arm64'
    if (/\b(arm|aarch32)\b/i.test(ua) && !/arm64/i.test(ua)) return 'arm'
    if (/\b(Wow64|Win64|x64|x86_64|amd64)\b/i.test(ua)) return 'x64'
    if (/\b(WOW32|Win32|x86)\b/i.test(ua)) return 'x86'
    return 'other'
  }

  function getLinuxFlavor() {
    if (getOS() !== 'linux') return null
    if (/\bUbuntu\b/i.test(ua)) return 'Ubuntu'
    if (/\bFedora\b/i.test(ua)) return 'Fedora'
    if (/\bDebian\b/i.test(ua)) return 'Debian'
    if (/\bLinux Mint\b/i.test(ua)) return 'Linux Mint'
    if (/\bOpenSUSE\b/i.test(ua)) return 'OpenSUSE'
    if (/\bArch\b/i.test(ua)) return 'Arch'
    if (/\bCentOS\b/i.test(ua)) return 'CentOS'
    if (/\bRed Hat\b/i.test(ua)) return 'Red Hat'
    if (/\bChrome OS\b/i.test(ua) || /\bCrOS\b/i.test(ua)) return 'Chrome OS'
    return null
  }

  function parseAssetFilename(name) {
    if (!name || typeof name !== 'string') return null
    const lower = name.toLowerCase()
    let platform = null
    if (lower.includes('windows')) platform = 'windows'
    else if (lower.includes('macos') || (lower.includes('mac') && !lower.includes('macos'))) platform = 'mac'
    else if (lower.includes('linux')) platform = 'linux'
    if (!platform) return null
    let arch = null
    if (lower.includes('arm64') || lower.includes('aarch64')) arch = 'arm64'
    else if (lower.includes('amd64') || lower.includes('x64')) arch = 'x64'
    else if (lower.includes('x86') || lower.includes('i386')) arch = 'x86'
    if (!arch) return null
    let packageType = null
    if (lower.endsWith('.deb')) packageType = 'deb'
    else if (lower.endsWith('.rpm')) packageType = 'rpm'
    else if (lower.endsWith('.dmg')) packageType = 'dmg'
    else if (lower.endsWith('.zip') || lower.endsWith('.msi')) packageType = 'archive'
    return { platform, arch, packageType: packageType || 'unknown', name }
  }

  function recommendDownload(assets, overrides) {
    const os = (overrides && overrides.os != null) ? overrides.os : getOS()
    const arch = (overrides && overrides.architecture != null) ? overrides.architecture : getArchitecture()
    const linuxFlavor = (overrides && overrides.linuxFlavor !== undefined) ? overrides.linuxFlavor : getLinuxFlavor()
    let reason = ''
    if (!Array.isArray(assets) || assets.length === 0) {
      return { recommended: null, alternatives: [], reason: reason || 'No assets provided.' }
    }
    const wantArch = (arch === 'x64' || arch === 'x86') ? 'x64' : (arch === 'arm64' || arch === 'arm') ? 'arm64' : null
    const wantOs = os === 'windows' ? 'windows' : os === 'mac' ? 'mac' : os === 'linux' ? 'linux' : null
    const preferDeb = linuxFlavor === 'Ubuntu' || linuxFlavor === 'Debian' || linuxFlavor === 'Linux Mint' || linuxFlavor === 'Chrome OS'
    const preferRpm = linuxFlavor === 'Fedora' || linuxFlavor === 'Red Hat' || linuxFlavor === 'CentOS' || linuxFlavor === 'OpenSUSE'
    const parsed = []
    for (let i = 0; i < assets.length; i++) {
      const a = assets[i]
      const name = a && (a.name != null ? a.name : a)
      const url = a && a.url
      const p = parseAssetFilename(name)
      if (!p) continue
      parsed.push({ name, url, parsed: p })
    }
    if (parsed.length === 0) {
      return { recommended: null, alternatives: [], reason: reason || 'No recognizable asset filenames.' }
    }
    function score(entry) {
      const p = entry.parsed
      const nameLower = (entry.name || '').toLowerCase()
      let s = 0
      if (wantOs && p.platform === wantOs) s += 100
      else if (wantOs) s -= 50
      if (wantArch && p.arch === wantArch) s += 50
      else if (wantArch) s -= 30
      if (wantOs === 'linux' && p.packageType === 'deb' && preferDeb) s += 20
      if (wantOs === 'linux' && p.packageType === 'rpm' && preferRpm) s += 20
      if (wantOs === 'linux' && !preferDeb && !preferRpm && p.packageType === 'deb') s += 5
      if (nameLower.includes('chardonnay')) s += 10
      if (nameLower.includes('classic')) s -= 5
      return s
    }
    parsed.sort((a, b) => score(b) - score(a))
    const best = parsed[0]
    const bestScore = score(best)
    const recommended = bestScore >= 50 ? { name: best.name, url: best.url } : null
    let altCandidates = parsed.slice(1, 4)
    if (wantOs) altCandidates = altCandidates.filter(e => e.parsed.platform === wantOs)
    if (wantArch) altCandidates = altCandidates.filter(e => e.parsed.arch === wantArch)
    const alternatives = altCandidates.map(e => ({ name: e.name, url: e.url }))
    const classicVsChardonnayFaqUrl = 'https://getlibation.com/docs/frequently-asked-questions#what-s-the-difference-between-classic-and-chardonnay'
    let windowsX64Choice = null
    if (wantOs === 'windows' && wantArch === 'x64') {
      let winChardonnay = null
      let winClassic = null
      for (let j = 0; j < parsed.length; j++) {
        const e = parsed[j]
        if (e.parsed.platform !== 'windows' || e.parsed.arch !== 'x64') continue
        const n = (e.name || '').toLowerCase()
        if (n.includes('chardonnay')) winChardonnay = { name: e.name, url: e.url }
        if (n.includes('classic')) winClassic = { name: e.name, url: e.url }
      }
      if (winChardonnay && winClassic) windowsX64Choice = { chardonnay: winChardonnay, classic: winClassic }
    }
    if (!recommended && wantOs) {
      reason = 'No asset matched your system (OS: ' + os + ', arch: ' + arch + '). Try choosing a download manually.'
    } else if (recommended && wantOs && !wantArch) {
      reason = 'Matched your OS; architecture could not be detected. If this download is wrong, pick another (e.g. arm64 vs x64).'
    }
    return {
      recommended,
      alternatives,
      reason: reason || (recommended ? 'Recommended for your system.' : ''),
      windowsX64Choice,
      classicVsChardonnayFaqUrl: windowsX64Choice ? classicVsChardonnayFaqUrl : null,
    }
  }

  return {
    os: getOS(),
    osVersion: getOSVersion(),
    architecture: getArchitecture(),
    linuxFlavor: getLinuxFlavor(),
    getOS,
    getOSVersion,
    getArchitecture,
    getLinuxFlavor,
    parseAssetFilename,
    recommendDownload,
  }
}
