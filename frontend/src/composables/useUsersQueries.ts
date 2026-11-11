import type { MaybeRefOrGetter } from 'vue'
import { usersService } from '@/services/users.service'
import { queryKeys } from './queryKeys'
import type { CreateUserRequestDto, QueryParameters, UpdateUserRequestDto } from '@/types/api'

export function useUsersQuery(
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.users.list(toValue(params))),
    queryFn: () => usersService.getAll(toValue(params)),
    enabled: computed(() => toValue(enabled)),
  })
}

export function useUserDetailQuery(
  userId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.users.detail(toValue(userId))),
    queryFn: () => usersService.getById(toValue(userId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(userId)) > 0),
  })
}

export function useCreateUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateUserRequestDto) => usersService.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useUpdateUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ userId, payload }: { userId: number; payload: UpdateUserRequestDto }) =>
      usersService.update(userId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users.detail(variables.userId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.lists() })
    },
  })
}

export function useDeleteUserMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (userId: number) => usersService.remove(userId),
    onSuccess: (_data, userId) => {
      queryClient.removeQueries({ queryKey: queryKeys.users.detail(userId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}
