import axios from 'axios'
import { jwtDecode } from 'jwt-decode'
import router from '@/router'
import { useAuthStore } from '@/stores/auth.ts'

interface JwtPayload {
  exp: number
  sub: string
  [key: string]: unknown
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://api.example.com',
  timeout: 10000,
})

export function isTokenValid(token: string): boolean {
  try {
    const decoded = jwtDecode<JwtPayload>(token)
    const currentTime = Date.now() / 1000
    return decoded.exp > currentTime
  } catch {
    return false
  }
}

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token && isTokenValid(token)) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      const authStore = useAuthStore()
      authStore.logout()
      router.push({ name: 'home' })
    }
    return Promise.reject(error)
  },
)

// TODO upravit podle skutečné implementace
