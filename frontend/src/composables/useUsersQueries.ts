import type { MaybeRefOrGetter } from 'vue'
import { usersService } from '@/services/users.service'
import { useAuthStore } from '@/stores/auth'
import { queryKeys } from './queryKeys'
import type { QueryParameters, UpdateUserRequestDto } from '@/types/api'

export function useMyCoursesQuery(
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.users.myCourses(toValue(params))),
    queryFn: () => usersService.getMyCourses(toValue(params)),
    enabled: computed(() => toValue(enabled)),
  })
}

export function useMyProgressQuery(
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.users.myProgress(toValue(params))),
    queryFn: () => usersService.getMyProgress(toValue(params)),
    enabled: computed(() => toValue(enabled)),
  })
}

// Kompletní historie progressu přihlášeného uživatele
export function useAllMyProgressQuery() {
  return useQuery({
    queryKey: queryKeys.users.allMyProgress(),
    staleTime: 60_000,
    queryFn: async () => {
      const allProgress: Awaited<ReturnType<typeof usersService.getMyProgress>>['data'] = []
      const maxPages = 100

      for (let page = 1; page <= maxPages; page++) {
        const response = await usersService.getMyProgress({
          Page: page,
          PageSize: 100,
        })

        const pagination = response.metadata?.pagination
        if (!pagination || typeof pagination.hasNextPage !== 'boolean') {
          throw new Error('Missing progress pagination metadata')
        }

        allProgress.push(...response.data)

        if (!pagination.hasNextPage) {
          return allProgress
        }

        if (pagination.totalPages != null && page >= pagination.totalPages) {
          throw new Error('Invalid progress pagination')
        }
      }

      throw new Error('Progress pagination limit exceeded')
    },
  })
}

export function useUpdateCurrentUserMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: UpdateUserRequestDto) => usersService.updateCurrent(payload),
    onSuccess: async () => {
      await authStore.refreshTokens()
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.users() })
    },
  })
}

export function useDeleteCurrentUserMutation() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => usersService.removeCurrent(),
    onSuccess: () => {
      authStore.clearAuth()
      queryClient.clear()
    },
  })
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: number; payload: UpdateUserRequestDto }) =>
      usersService.update(userId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(variables.userId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.users() })
    },
  })
}

export function useDeleteUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (userId: number) => usersService.remove(userId),
    onSuccess: (_data, userId) => {
      queryClient.removeQueries({ queryKey: queryKeys.admin.userDetail(userId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.users() })
    },
  })
}
