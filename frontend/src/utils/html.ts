// src/utils/html.ts
import DOMPurify from 'dompurify'

let hookRegistered = false

function ensureTargetBlankHook() {
  if (hookRegistered) return
  DOMPurify.addHook('afterSanitizeAttributes', (node) => {
    if (node instanceof Element && node.getAttribute('target') === '_blank') {
      node.setAttribute('rel', 'noopener noreferrer')
    }
  })
  hookRegistered = true
}

/**
 * Bezpečně vyčistí HTML výstup z OpenVue Editoru před vykreslením přes v-html
 */
export function sanitizeHtml(dirtyHtml: string | undefined | null): string {
  if (!dirtyHtml) return ''
  ensureTargetBlankHook()
  return DOMPurify.sanitize(dirtyHtml, {
    USE_PROFILES: { html: true },
    ADD_ATTR: ['target', 'rel'],
  })
}

/**
 * Převede běžný YouTube odkaz (watch?v= nebo youtu.be/) na embed URL pro <iframe>
 */
export function getYoutubeEmbedUrl(url: string | undefined | null): string | null {
  if (!url) return null
  const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=)([^#&?]*).*/
  const match = url.match(regExp)
  return match && match[2]!.length === 11 ? `https://www.youtube.com/embed/${match[2]}` : null
}
