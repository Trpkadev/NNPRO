<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { formatDateTime } from '@/utils/formatters'

const props = defineProps<{
  userId: number
}>()

const { t } = useI18n()
const toast = useToast()

const sessionsPage = ref(1)
const revokingSessionId = ref<string | null>(null)
const errorMessage = ref<string | null>(null)

watch(
  () => props.userId,
  () => {
    sessionsPage.value = 1
  },
)

const { data: sessionsResponse, isLoading: sessionsLoading } = useAdminUserSessionsQuery(
  computed(() => props.userId),
  computed(() => ({ Page: sessionsPage.value, PageSize: 10 })),
)

const revokeSessionMutation = useRevokeUserSessionMutation()
const revokeAllMutation = useRevokeAllUserSessionsMutation()

async function handleRevokeSession(sessionId: string) {
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
  <div class="space-y-3">
    <Message v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

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
          :loading="
            revokeSessionMutation.isPending.value && revokingSessionId === session.sessionId
          "
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
</template>
