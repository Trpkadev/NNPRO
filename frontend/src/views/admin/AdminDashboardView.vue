<script setup lang="ts">
const { data: usersResponse, isLoading } = useUsersQuery({ Page: 1, PageSize: 20 })
const users = computed(() => usersResponse.value?.data ?? [])
</script>

<template>
  <div class="space-y-4">
    <div v-if="isLoading" class="space-y-2">
      <Skeleton v-for="n in 4" :key="n" height="3.5rem" class="rounded-xl" />
    </div>

    <div
      v-else
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden"
    >
      <DataTable :value="users" striped-rows responsive-layout="scroll">
        <Column field="id" header="ID" />
        <Column field="username" header="Uživatelské jméno" />
        <Column field="email" header="E-mail" />
        <Column field="role" header="Role">
          <template #body="{ data }">
            <Tag :value="data.role" />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

