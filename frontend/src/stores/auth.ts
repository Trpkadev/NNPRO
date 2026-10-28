export const useAuthStore = defineStore('auth', () => {
  const token = useStorage<string | null>('access_token', null) // Reaktivní vazba přes VueUse
  const isAuthenticated = computed(() => !!token.value && isTokenValid(token.value))

  function setToken(newToken: string | null) {
    token.value = newToken
  }

  function logout() {
    token.value = null
  }

  return { token, isAuthenticated, setToken, logout }
})
