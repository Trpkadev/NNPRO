<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()
const router = useRouter()
const toast = useToast()

const registerMutation = useRegisterMutation()
const serverError = ref<string | null>(null)

const { validate, values, errors, defineField, setFieldError } = useForm({
  validationSchema: {
    username: (value: string) => value.trim().length >= 3 || t('validation.minLength', { min: 3 }),
    email: (value: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || t('validation.email'),
    password: (value: string) => value.length >= 6 || t('validation.minLength', { min: 6 }),
    confirmPassword: (value: string) => value.length > 0 || t('validation.required'),
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

const onSubmit = async () => {
  serverError.value = null

  const validation = await validate()
  if (!validation.valid) return

  if (values.password !== values.confirmPassword) {
    setFieldError('confirmPassword', t('validation.passwordsDontMatch'))
    return
  }

  try {
    await registerMutation.mutateAsync({
      username: values.username.trim(),
      email: values.email.trim(),
      password: values.password,
    })

    toast.add({
      severity: 'success',
      summary: t('auth.registerSuccess'),
      life: 3000,
    })

    await router.replace({ name: 'home' })
  } catch (err) {
    serverError.value = getApiErrorMessage(err, t('auth.registerError'))
  }
}
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
