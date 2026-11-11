import { api } from './api'
import type {
  AuthResponseDto,
  LoginRequestDto,
  RegisterRequestDto,
  WrapperOfAuthResponseDto,
} from '@/types/api'

export const authService = {
  /**
   * Přihlášení uživatele (POST /api/auth/login)
   * Nastaví HttpOnly refreshToken cookie a vrátí access token.
   */
  async login(payload: LoginRequestDto): Promise<AuthResponseDto> {
    const body: LoginRequestDto = {
      username: payload.username ?? null,
      email: payload.email ?? null,
      password: payload.password,
      ...(payload.deviceName !== undefined ? { deviceName: payload.deviceName } : {}),
    }
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/login', body)
    return data.data
  },

  /**
   * Registrace nového uživatele (POST /api/auth/register)
   * Nastaví HttpOnly refreshToken cookie a vrátí access token.
   */
  async register(payload: RegisterRequestDto): Promise<AuthResponseDto> {
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/register', payload)
    return data.data
  },

  /**
   * Obnovení access tokenu pomocí HttpOnly refreshToken cookie (POST /api/auth/refresh)
   */
  async refresh(): Promise<AuthResponseDto> {
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/refresh')
    return data.data
  },

  /**
   * Odhlášení uživatele a revokace refreshToken cookie / relace (POST /api/auth/logout)
   */
  async logout(): Promise<void> {
    await api.post('/api/auth/logout')
  },
}
