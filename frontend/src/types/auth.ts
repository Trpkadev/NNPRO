import type { UserRole } from './api'

/**
 * Struktura JWT payloadu generovaného backendem (ELearningApi)
 */
export interface JwtPayload {
  /** ID uživatele (userId) jako řetězec */
  sub: string
  /** Unikátní identifikátor tokenu (jwtId) */
  jti: string
  /** Čas vydání tokenu (IssuedAt - Unix timestamp v sekundách) */
  iat: number
  /** Uživatelské jméno (user.username) */
  name: string
  /** Role uživatele (user.role) */
  role: UserRole
  /** Čas vypršení platnosti tokenu (expiresAt - Unix timestamp v sekundách) */
  exp: number
  /** Vydavatel tokenu (ELearningApi) */
  iss: string
  /** Cílová skupina tokenu (ELearningClient) */
  aud: string
}

export interface AuthenticatedUser {
  id: number
  username: string
  role: UserRole
}

export type LayoutType = 'main' | 'auth' | 'admin' | 'instructor' | 'empty'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    guestOnly?: boolean
    roles?: UserRole[]
    layout?: LayoutType
    titleKey?: string
  }
}
