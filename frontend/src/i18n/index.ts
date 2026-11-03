import { createI18n } from 'vue-i18n'

const messages = {
  cs: {
    welcome: 'Vítejte v aplikaci',
    notFound: {
      title: 'Stránka nenalezena',
      description: 'Omlouváme se, ale stránka, kterou hledáte, neexistuje nebo byla přesunuta.',
      goBack: 'Zpět',
      goHome: 'Domů',
    },
    validation: {
      required: 'Toto pole je povinné',
      email: 'Zadejte platný e-mail',
    },
  },
  en: {
    welcome: 'Welcome to the app',
    notFound: {
      title: 'Page not found',
      description: 'Sorry, the page you are looking for does not exist or has been moved.',
      goBack: 'Go back',
      goHome: 'Home',
    },
    validation: {
      required: 'This field is required',
      email: 'Enter a valid email',
    },
  },
}

export const i18n = createI18n({
  legacy: false, // Nutné pro Composition API
  locale: 'cs',
  fallbackLocale: 'en',
  messages,
})
