import type { MaybeRefOrGetter } from 'vue'
import { usersService } from '@/services/users.service'
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

export function useUpdateCurrentUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: UpdateUserRequestDto) => usersService.updateCurrent(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.users() })
    },
  })
}

export function useDeleteCurrentUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => usersService.removeCurrent(),
    onSuccess: () => {
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
