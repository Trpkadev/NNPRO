<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const toast = useToast()

const loginMutation = useLoginMutation()
const serverError = ref<string | null>(null)

const { handleSubmit, errors, defineField } = useForm({
  validationSchema: {
    identifier: (value: string) => value.trim().length > 0 || t('validation.required'),
    password: (value: string) => value.length > 0 || t('validation.required'),
  },
  initialValues: {
    identifier: '',
    password: '',
  },
})

const [identifier, identifierAttrs] = defineField('identifier')
const [password, passwordAttrs] = defineField('password')

const onSubmit = handleSubmit(async (values) => {
  serverError.value = null
  const trimmedIdentifier = values.identifier.trim()
  const isEmail = trimmedIdentifier.includes('@')

  try {
    await loginMutation.mutateAsync({
      email: isEmail ? trimmedIdentifier : null,
      username: isEmail ? null : trimmedIdentifier,
      password: values.password,
    })

    toast.add({
      severity: 'success',
      summary: t('auth.loginSuccess'),
      life: 3000,
    })

    const redirectPath = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.replace(redirectPath)
  } catch (err) {
    serverError.value = getApiErrorMessage(err, t('auth.loginError'))
  }
})
</script>

<template>
  <div class="space-y-6">
    <div class="text-center space-y-1.5">
      <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
        {{ t('auth.loginTitle') }}
      </h1>
      <p class="text-sm text-surface-500 dark:text-surface-400">
        {{ t('auth.loginSubtitle') }}
      </p>
    </div>

    <Message v-if="serverError" severity="error" :closable="false">
      {{ serverError }}
    </Message>

    <form class="space-y-4" @submit.prevent="onSubmit">
      <div class="space-y-1.5">
        <label for="identifier" class="block text-sm font-medium">
          {{ t('auth.identifierLabel') }}
        </label>
        <InputText
          id="identifier"
          v-model="identifier"
          v-bind="identifierAttrs"
          :placeholder="t('auth.identifierPlaceholder')"
          :invalid="!!errors.identifier"
          autocomplete="username"
          class="w-full"
        />
        <small v-if="errors.identifier" class="text-red-500 text-xs block">
          {{ errors.identifier }}
        </small>
      </div>

      <div class="space-y-1.5">
        <label for="password" class="block text-sm font-medium">
          {{ t('auth.passwordLabel') }}
        </label>
        <Password
          input-id="password"
          v-model="password"
          v-bind="passwordAttrs"
          :placeholder="t('auth.passwordPlaceholder')"
          :feedback="false"
          toggle-mask
          :invalid="!!errors.password"
          autocomplete="current-password"
          fluid
        />
        <small v-if="errors.password" class="text-red-500 text-xs block">
          {{ errors.password }}
        </small>
      </div>

      <Button
        type="submit"
        :label="t('auth.loginSubmit')"
        icon="oi oi-sign-in"
        class="w-full mt-2"
        :loading="loginMutation.isPending.value"
      />
    </form>

    <div
      class="pt-4 border-t border-surface-200 dark:border-surface-800 text-center text-sm text-surface-600 dark:text-surface-400"
    >
      {{ t('auth.noAccount') }}
      <RouterLink :to="{ name: 'register' }" class="text-primary font-semibold hover:underline">
        {{ t('auth.createAccountLink') }}
      </RouterLink>
    </div>
  </div>
</template>
