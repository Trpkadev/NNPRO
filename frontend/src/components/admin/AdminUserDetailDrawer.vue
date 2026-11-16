<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { formatDateTime } from '@/utils/formatters'
import { UserRoles, type UserRole } from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  userId: number | null
}>()

const { t } = useI18n()
const toast = useToast()

const activeUserId = computed(() => props.userId ?? 0)
const isEnabled = computed(() => visible.value && activeUserId.value > 0)
const sessionsPage = ref(1)

watch(
  () => props.userId,
  () => {
    sessionsPage.value = 1
  },
)

const { data: userDetail, isLoading: detailLoading } = useAdminUserDetailQuery(
  activeUserId,
  isEnabled,
)

const { data: sessionsResponse, isLoading: sessionsLoading } = useAdminUserSessionsQuery(
  activeUserId,
  computed(() => ({ Page: sessionsPage.value, PageSize: 10 })),
  isEnabled,
)

const updateRoleMutation = useUpdateUserRoleMutation()
const updateUserMutation = useUpdateUserMutation()
const revokeSessionMutation = useRevokeUserSessionMutation()
const revokeAllMutation = useRevokeAllUserSessionsMutation()

const selectedRole = ref<UserRole>(UserRoles.Student)
const editUsername = ref('')
const editEmail = ref('')
const editPassword = ref('')
const revokingSessionId = ref<string | null>(null)
const errorMessage = ref<string | null>(null)

const roleOptions = computed(() => [
  { label: t('roles.Admin'), value: UserRoles.Admin },
  { label: t('roles.Instructor'), value: UserRoles.Instructor },
  { label: t('roles.Student'), value: UserRoles.Student },
])

watch(
  userDetail,
  (detail) => {
    if (detail) {
      selectedRole.value = (detail.role as UserRole) || UserRoles.Student
      editUsername.value = detail.username
      editEmail.value = detail.email
      editPassword.value = ''
      errorMessage.value = null
    }
  },
  { immediate: true },
)

async function handleRoleChange() {
  if (!props.userId) return
  errorMessage.value = null
  try {
    await updateRoleMutation.mutateAsync({
      userId: props.userId,
      payload: { role: selectedRole.value },
    })
    toast.add({
      severity: 'success',
      summary: t('admin.roleUpdatedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleUpdateUser() {
  if (!props.userId) return
  errorMessage.value = null
  if (!editUsername.value.trim() || !editEmail.value.trim()) {
    errorMessage.value = t('validation.required')
    return
  }
  if (editPassword.value.length > 0 && editPassword.value.length < 8) {
    errorMessage.value = t('validation.minLength', { min: 8 })
    return
  }

  try {
    await updateUserMutation.mutateAsync({
      userId: props.userId,
      payload: {
        username: editUsername.value.trim(),
        email: editEmail.value.trim(),
        password: editPassword.value,
      },
    })
    editPassword.value = ''
    toast.add({
      severity: 'success',
      summary: t('admin.userUpdatedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleRevokeSession(sessionId: string) {
  if (!props.userId) return
  errorMessage.value = null
  revokingSessionId.value = sessionId
  try {
    await revokeSessionMutation.mutateAsync({
      userId: props.userId,
      sessionId,
    })
    toast.add({
      severity: 'info',
      summary: t('admin.sessionsRevokedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  } finally {
    revokingSessionId.value = null
  }
}

async function handleRevokeAll() {
  if (!props.userId) return
  errorMessage.value = null
  try {
    await revokeAllMutation.mutateAsync(props.userId)
    toast.add({
      severity: 'info',
      summary: t('admin.sessionsRevokedSuccess'),
      life: 3000,
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Drawer
    v-model:visible="visible"
    position="right"
    :header="t('admin.userDetailsTitle')"
    class="!w-full sm:!w-[34rem]"
  >
    <div v-if="detailLoading" class="space-y-3">
      <Skeleton height="5rem" class="rounded-xl" />
      <Skeleton height="8rem" class="rounded-xl" />
      <Skeleton height="10rem" class="rounded-xl" />
    </div>

    <div v-else-if="userDetail" class="space-y-6">
      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <!-- Statistiky uživatele -->
      <div class="grid grid-cols-2 gap-3">
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.enrollmentsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.enrollmentCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.completedLessonsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.lessonProgressCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.quizAttemptsCount') }}</span>
          <span class="text-xl font-bold">{{ userDetail.quizAttemptCount }}</span>
        </div>
        <div class="p-3.5 rounded-xl bg-surface-100 dark:bg-surface-800">
          <span class="text-xs text-surface-500 block">{{ t('admin.activeSessionsCount') }}</span>
          <span class="text-xl font-bold text-primary">{{ userDetail.activeSessionCount }}</span>
        </div>
      </div>

      <!-- Změna role (RBAC) -->
      <div class="p-4 rounded-xl border border-surface-200 dark:border-surface-800 space-y-3">
        <label for="admin-user-role-select" class="block text-sm font-bold">
          {{ t('admin.changeRoleLabel') }}
        </label>
        <div class="flex items-center gap-2">
          <Select
            label-id="admin-user-role-select"
            v-model="selectedRole"
            :options="roleOptions"
            option-label="label"
            option-value="value"
            size="small"
            class="flex-1"
          />
          <Button
            :label="t('common.save')"
            icon="oi oi-check"
            size="small"
            :loading="updateRoleMutation.isPending.value"
            @click="handleRoleChange"
          />
        </div>
      </div>

      <!-- Úprava údajů uživatele -->
      <form
        class="p-4 rounded-xl border border-surface-200 dark:border-surface-800 space-y-3"
        @submit.prevent="handleUpdateUser"
      >
        <h4 class="text-sm font-bold">{{ t('admin.editUserTitle') }}</h4>
        <div class="space-y-1">
          <label for="admin-edit-username" class="block text-xs font-medium">
            {{ t('admin.username') }}
          </label>
          <InputText id="admin-edit-username" v-model="editUsername" size="small" class="w-full" required />
        </div>
        <div class="space-y-1">
          <label for="admin-edit-email" class="block text-xs font-medium">
            {{ t('admin.email') }}
          </label>
          <InputText id="admin-edit-email" v-model="editEmail" type="email" size="small" class="w-full" required />
        </div>
        <div class="space-y-1">
          <label for="admin-edit-password" class="block text-xs font-medium">
            {{ t('profile.newPasswordLabel') }}
          </label>
          <Password
            input-id="admin-edit-password"
            v-model="editPassword"
            :feedback="false"
            toggle-mask
            fluid
          />
        </div>
        <div class="flex justify-end">
          <Button
            type="submit"
            :label="t('common.save')"
            icon="oi oi-check"
            size="small"
            :loading="updateUserMutation.isPending.value"
          />
        </div>
      </form>

      <!-- Aktivní relace uživatele -->
      <div class="space-y-3">
        <div class="flex items-center justify-between gap-2">
          <h4 class="text-sm font-bold">{{ t('admin.sessionsTitle') }}</h4>
          <Button
            v-if="(sessionsResponse?.data ?? []).length > 0"
            :label="t('admin.revokeAllSessions')"
            icon="oi oi-sign-out"
            severity="danger"
            outlined
            size="small"
            :loading="revokeAllMutation.isPending.value"
            @click="handleRevokeAll"
          />
        </div>

        <div v-if="sessionsLoading" class="space-y-2">
          <Skeleton v-for="n in 2" :key="n" height="4rem" class="rounded-xl" />
        </div>

        <p
          v-else-if="(sessionsResponse?.data ?? []).length === 0"
          class="text-xs text-surface-500 italic py-2"
        >
          {{ t('admin.noActiveSessions') }}
        </p>

        <div v-else class="space-y-2">
          <div
            v-for="session in sessionsResponse?.data ?? []"
            :key="session.sessionId"
            class="p-3 rounded-xl border border-surface-200 dark:border-surface-800 flex items-center justify-between gap-3 text-xs"
          >
            <div class="space-y-0.5 min-w-0">
              <p class="font-semibold truncate">
                {{ session.deviceName || session.userAgent || session.sessionId }}
              </p>
              <p class="text-surface-500">
                {{ t('admin.ipAddress') }}: {{ session.createdByIp || '-' }} &bull;
                {{ t('admin.expiresAt') }}: {{ formatDateTime(session.expiresAtUtc) }}
              </p>
            </div>

            <Button
              :label="t('admin.revokeSession')"
              severity="danger"
              text
              size="small"
              :loading="revokeSessionMutation.isPending.value && revokingSessionId === session.sessionId"
              @click="handleRevokeSession(session.sessionId)"
            />
          </div>

          <PaginationControls
            :pagination="sessionsResponse?.metadata?.pagination"
            :loading="sessionsLoading"
            @update:page="sessionsPage = $event"
          />
        </div>
      </div>
    </div>
  </Drawer>
</template>
