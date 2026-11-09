import { api } from './api'
import type {
  AuthResponseDto,
  LoginRequestDto,
  LogoutRequestDto,
  RefreshTokenRequestDto,
  RegisterRequestDto,
  WrapperOfAuthResponseDto,
} from '@/types/api'

export const authService = {
  /**
   * Přihlášení uživatele (POST /api/auth/login)
   */
  async login(payload: LoginRequestDto): Promise<AuthResponseDto> {
    const body: LoginRequestDto = {
      username: payload.username ?? null,
      email: payload.email ?? null,
      password: payload.password,
    }
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/login', body)
    return data.data
  },

  /**
   * Registrace nového uživatele (POST /api/auth/register)
   */
  async register(payload: RegisterRequestDto): Promise<AuthResponseDto> {
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/register', payload)
    return data.data
  },

  /**
   * Obnovení access tokenu pomocí refresh tokenu (POST /api/auth/refresh)
   */
  async refresh(payload: RefreshTokenRequestDto): Promise<AuthResponseDto> {
    const { data } = await api.post<WrapperOfAuthResponseDto>('/api/auth/refresh', payload)
    return data.data
  },

  /**
   * Odhlášení uživatele a revokace refresh tokenu (POST /api/auth/logout)
   */
  async logout(payload: LogoutRequestDto): Promise<void> {
    await api.post('/api/auth/logout', payload)
  },
}

