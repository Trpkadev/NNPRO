<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import { UserRoles, type UserResponseDto, type UserRole } from '@/types/api'

const { t } = useI18n()
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()

const page = ref(1)
const filterUsername = ref('')
const filterEmail = ref('')
const filterRole = ref<UserRole | ''>('')
const debouncedUsername = refDebounced(filterUsername, 350)
const debouncedEmail = refDebounced(filterEmail, 350)
const sortBy = ref<'Id' | 'Username' | 'Email' | 'Role'>('Id')
const descending = ref(false)

watch([debouncedUsername, debouncedEmail, filterRole, sortBy, descending], () => {
  page.value = 1
})

const queryParams = computed(() => ({
  Page: page.value,
  PageSize: 15,
  SortBy: sortBy.value,
  Descending: descending.value,
  Filters: {
    username: debouncedUsername.value,
    email: debouncedEmail.value,
    role: filterRole.value || undefined,
  },
}))

const { data: usersResponse, isLoading, isError, error } = useAdminUsersQuery(queryParams)
const deleteUserMutation = useDeleteUserMutation()

const users = computed(() => usersResponse.value?.data ?? [])
const pagination = computed(() => usersResponse.value?.metadata?.pagination ?? null)
const usersError = computed(() => getApiErrorMessage(error.value, t('admin.usersLoadError')))

const roleFilterOptions = computed(() => [
  { label: t('admin.filterRolePlaceholder'), value: '' },
  { label: t('roles.Admin'), value: UserRoles.Admin },
  { label: t('roles.Instructor'), value: UserRoles.Instructor },
  { label: t('roles.Student'), value: UserRoles.Student },
])

const selectedUserId = ref<number | null>(null)
const detailDrawerVisible = ref(false)

function openUserDetail(userId: number) {
  selectedUserId.value = userId
  detailDrawerVisible.value = true
}

function getRoleSeverity(role: string) {
  if (role === UserRoles.Admin) return 'danger'
  if (role === UserRoles.Instructor) return 'warn'
  return 'info'
}

function confirmDeleteUser(user: UserResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('admin.deleteUserConfirm', { username: user.username }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteUserMutation.mutateAsync(user.id)
        toast.add({
          severity: 'info',
          summary: t('admin.userDeletedSuccess'),
          life: 3000,
        })
      } catch (err) {
        toast.add({
          severity: 'error',
          summary: getApiErrorMessage(err),
          life: 4000,
        })
      }
    },
  })
}

function clearFilters() {
  filterUsername.value = ''
  filterEmail.value = ''
  filterRole.value = ''
  sortBy.value = 'Id'
  descending.value = false
}
</script>

<template>
  <div class="space-y-6">
    <!-- Filtry, řazení a přepnutí do správy kurzů -->
    <div
      class="p-4 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-wrap items-center justify-between gap-3"
    >
      <div class="flex flex-wrap items-center gap-2 flex-1">
        <InputText
          v-model="filterUsername"
          :placeholder="t('admin.filterUsernamePlaceholder')"
          size="small"
          class="w-full sm:w-48"
        />
        <InputText
          v-model="filterEmail"
          :placeholder="t('admin.filterEmailPlaceholder')"
          size="small"
          class="w-full sm:w-52"
        />
        <Select
          v-model="filterRole"
          :options="roleFilterOptions"
          option-label="label"
          option-value="value"
          size="small"
          class="w-full sm:w-44"
        />
        <Button
          v-if="filterUsername || filterEmail || filterRole"
          :label="t('common.clearFilters')"
          icon="oi oi-times"
          severity="secondary"
          text
          size="small"
          @click="clearFilters"
        />
      </div>

      <div class="flex items-center gap-2">
        <Button
          :label="t('nav.coursesManagement')"
          icon="oi oi-briefcase"
          severity="warn"
          outlined
          size="small"
          @click="router.push({ name: 'instructor-dashboard' })"
        />
      </div>
    </div>

    <!-- Načítání -->
    <div v-if="isLoading" class="space-y-2">
      <Skeleton v-for="n in 5" :key="n" height="3.5rem" class="rounded-xl" />
    </div>

    <div v-else-if="isError">
      <Message severity="error" :closable="false">{{ usersError }}</Message>
    </div>

    <!-- Tabulka uživatelů se stránkováním -->
    <div
      v-else
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden p-4 space-y-4"
    >
      <DataTable :value="users" striped-rows responsive-layout="scroll">
        <Column field="id" :header="t('admin.id')" />
        <Column field="username" :header="t('admin.username')">
          <template #body="{ data }">
            <span class="font-semibold">{{ data.username }}</span>
          </template>
        </Column>
        <Column field="email" :header="t('admin.email')" />
        <Column field="role" :header="t('admin.role')">
          <template #body="{ data }">
            <Tag :value="data.role" :severity="getRoleSeverity(data.role)" />
          </template>
        </Column>
        <Column :header="t('common.actions')">
          <template #body="{ data }">
            <div class="flex items-center gap-1.5">
              <Button
                :label="t('common.edit')"
                icon="oi oi-cog"
                size="small"
                severity="secondary"
                outlined
                @click="openUserDetail(data.id)"
              />
              <Button
                icon="oi oi-trash"
                severity="danger"
                text
                rounded
                size="small"
                :aria-label="t('common.delete')"
                @click="confirmDeleteUser(data)"
              />
            </div>
          </template>
        </Column>
      </DataTable>

      <PaginationControls
        :pagination="pagination"
        :loading="isLoading"
        @update:page="page = $event"
      />
    </div>

    <!-- Postranní panel detailu uživatele, změny role a správy relací -->
    <AdminUserDetailDrawer
      v-model:visible="detailDrawerVisible"
      :user-id="selectedUserId"
    />
  </div>
</template>
