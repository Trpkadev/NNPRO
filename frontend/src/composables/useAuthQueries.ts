import { useAuthStore } from '@/stores/auth'
import type { LoginRequestDto, RegisterRequestDto } from '@/types/api'

export function useLoginMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: LoginRequestDto) => authStore.login(payload),
    onSuccess: () => {
      queryClient.invalidateQueries()
    },
  })
}

export function useRegisterMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: RegisterRequestDto) => authStore.register(payload),
    onSuccess: () => {
      queryClient.invalidateQueries()
    },
  })
}

export function useLogoutMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => authStore.logout(),
    onSettled: () => {
      queryClient.clear()
    },
  })
}
