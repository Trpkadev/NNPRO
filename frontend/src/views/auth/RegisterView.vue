<script setup lang="ts">
import axios from 'axios'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const { t, locale } = useI18n()
const router = useRouter()
const toast = useToast()

const registerMutation = useRegisterMutation()
const submitError = ref<unknown>(null)
const serverError = computed(() => {
  if (!submitError.value) return null
  const isConflict =
    axios.isAxiosError(submitError.value) && submitError.value.response?.status === 409
  const fallback = isConflict ? t('auth.registerConflictError') : t('auth.registerError')
  return getApiErrorMessage(submitError.value, fallback)
})

const { handleSubmit, values, errors, defineField, validateField } = useForm({
  validationSchema: {
    username: (value: string) => {
      const trimmed = value.trim()
      if (trimmed.length < 3) return t('validation.minLength', { min: 3 })
      if (trimmed.includes('@')) return t('validation.usernameNoAt')
      return true
    },
    email: (value: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || t('validation.email'),
    password: (value: string) => value.length >= 8 || t('validation.minLength', { min: 8 }),
    confirmPassword: (value: string) => {
      if (value.length === 0) return t('validation.required')
      if (value !== values.password) return t('validation.passwordsDontMatch')
      return true
    },
  },
  initialValues: {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  },
})

const [username, usernameAttrs] = defineField('username')
const [email, emailAttrs] = defineField('email')
const [password, passwordAttrs] = defineField('password')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

watch(locale, () => {
  for (const field of Object.keys(errors.value) as Array<
    'username' | 'email' | 'password' | 'confirmPassword'
  >) {
    if (errors.value[field]) {
      validateField(field)
    }
  }
})

const onSubmit = handleSubmit(async (formValues) => {
  submitError.value = null

  try {
    await registerMutation.mutateAsync({
      username: formValues.username.trim(),
      email: formValues.email.trim(),
      password: formValues.password,
    })

    toast.add({
      severity: 'success',
      summary: t('auth.registerSuccess'),
      life: 3000,
    })

    await router.replace({ name: 'home' })
  } catch (err) {
    submitError.value = err
  }
})
</script>

<template>
  <div class="space-y-6">
    <div class="text-center space-y-1.5">
      <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
        {{ t('auth.registerTitle') }}
      </h1>
      <p class="text-sm text-surface-500 dark:text-surface-400">
        {{ t('auth.registerSubtitle') }}
      </p>
    </div>

    <Message v-if="serverError" severity="error" :closable="false">
      {{ serverError }}
    </Message>

    <form class="space-y-4" @submit.prevent="onSubmit">
      <div class="space-y-1.5">
        <label for="username" class="block text-sm font-medium">
          {{ t('auth.usernameLabel') }}
        </label>
        <InputText
          id="username"
          v-model="username"
          v-bind="usernameAttrs"
          :placeholder="t('auth.usernamePlaceholder')"
          :invalid="!!errors.username"
          autocomplete="username"
          class="w-full"
        />
        <small v-if="errors.username" class="text-red-500 text-xs block">
          {{ errors.username }}
        </small>
      </div>

      <div class="space-y-1.5">
        <label for="email" class="block text-sm font-medium">
          {{ t('auth.emailLabel') }}
        </label>
        <InputText
          id="email"
          v-model="email"
          v-bind="emailAttrs"
          type="email"
          :placeholder="t('auth.emailPlaceholder')"
          :invalid="!!errors.email"
          autocomplete="email"
          class="w-full"
        />
        <small v-if="errors.email" class="text-red-500 text-xs block">
          {{ errors.email }}
        </small>
      </div>

      <div class="space-y-1.5">
        <label for="reg-password" class="block text-sm font-medium">
          {{ t('auth.passwordLabel') }}
        </label>
        <Password
          input-id="reg-password"
          v-model="password"
          v-bind="passwordAttrs"
          :placeholder="t('auth.passwordPlaceholder')"
          toggle-mask
          :invalid="!!errors.password"
          autocomplete="new-password"
          fluid
        />
        <small v-if="errors.password" class="text-red-500 text-xs block">
          {{ errors.password }}
        </small>
      </div>

      <div class="space-y-1.5">
        <label for="confirm-password" class="block text-sm font-medium">
          {{ t('auth.confirmPasswordLabel') }}
        </label>
        <Password
          input-id="confirm-password"
          v-model="confirmPassword"
          v-bind="confirmPasswordAttrs"
          :placeholder="t('auth.confirmPasswordPlaceholder')"
          :feedback="false"
          toggle-mask
          :invalid="!!errors.confirmPassword"
          autocomplete="new-password"
          fluid
        />
        <small v-if="errors.confirmPassword" class="text-red-500 text-xs block">
          {{ errors.confirmPassword }}
        </small>
      </div>

      <Button
        type="submit"
        :label="t('auth.registerSubmit')"
        icon="oi oi-user-plus"
        class="w-full mt-2"
        :loading="registerMutation.isPending.value"
      />
    </form>

    <div
      class="pt-4 border-t border-surface-200 dark:border-surface-800 text-center text-sm text-surface-600 dark:text-surface-400"
    >
      {{ t('auth.haveAccount') }}
      <RouterLink :to="{ name: 'login' }" class="text-primary font-semibold hover:underline">
        {{ t('auth.loginLink') }}
      </RouterLink>
    </div>
  </div>
</template>
