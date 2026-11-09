import type { UserRole } from './api'

/**
 * Struktura JWT payloadu (podporuje jak standardní JWT claims, tak ASP.NET Core claims)
 */
export interface JwtPayload {
  exp?: number
  iat?: number
  nbf?: number
  sub?: string
  nameid?: string
  unique_name?: string
  name?: string
  email?: string
  role?: UserRole | UserRole[] | string | string[]
  roles?: UserRole[] | string[]
  // ASP.NET Core ClaimTypes
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?:
    | UserRole
    | UserRole[]
    | string
    | string[]
  [key: string]: unknown
}

export interface AuthenticatedUser {
  id: number | null
  username: string
  email: string
  role: UserRole | null
  roles: UserRole[]
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

