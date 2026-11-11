import type { MaybeRefOrGetter } from 'vue'
import { adminService } from '@/services/admin.service'
import { queryKeys } from './queryKeys'
import type { QueryParameters, UpdateUserRoleRequestDto } from '@/types/api'

export function useAdminUsersQuery(
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.admin.userList(toValue(params))),
    queryFn: () => adminService.getUsers(toValue(params)),
    enabled: computed(() => toValue(enabled)),
  })
}

export function useAdminUserDetailQuery(
  userId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.admin.userDetail(toValue(userId))),
    queryFn: () => adminService.getUserById(toValue(userId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(userId)) > 0),
  })
}

export function useAdminUserSessionsQuery(
  userId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.admin.userSessions(toValue(userId), toValue(params))),
    queryFn: () => adminService.getUserSessions(toValue(userId), toValue(params)),
    enabled: computed(() => toValue(enabled) && Number(toValue(userId)) > 0),
  })
}

export function useUpdateUserRoleMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: number; payload: UpdateUserRoleRequestDto }) =>
      adminService.updateUserRole(userId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(variables.userId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.users() })
    },
  })
}

export function useRevokeAllUserSessionsMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (userId: number) => adminService.revokeAllUserSessions(userId),
    onSuccess: (_data, userId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(userId) })
    },
  })
}

export function useRevokeUserSessionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, sessionId }: { userId: number; sessionId: string }) =>
      adminService.revokeUserSession(userId, sessionId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(variables.userId) })
    },
  })
}

