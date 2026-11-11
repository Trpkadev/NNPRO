<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const { data: usersResponse, isLoading, isError, error } = useAdminUsersQuery({
  Page: 1,
  PageSize: 20,
})
const users = computed(() => usersResponse.value?.data ?? [])
const usersError = computed(() => getApiErrorMessage(error.value, t('admin.usersLoadError')))
</script>

<template>
  <div class="space-y-4">
    <div v-if="isLoading" class="space-y-2">
      <Skeleton v-for="n in 4" :key="n" height="3.5rem" class="rounded-xl" />
    </div>

    <div v-else-if="isError">
      <Message severity="error" :closable="false">{{ usersError }}</Message>
    </div>

    <div
      v-else
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden"
    >
      <DataTable :value="users" striped-rows responsive-layout="scroll">
        <Column field="id" :header="t('admin.id')" />
        <Column field="username" :header="t('admin.username')" />
        <Column field="email" :header="t('admin.email')" />
        <Column field="role" :header="t('admin.role')">
          <template #body="{ data }">
            <Tag :value="data.role" />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
