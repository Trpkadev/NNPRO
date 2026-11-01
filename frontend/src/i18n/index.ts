import { createI18n } from 'vue-i18n'

const messages = {
  cs: {
    welcome: 'Vítejte v aplikaci',
    loading: {
      default: 'Načítáme obsah kurzu...',
      courses: 'Načítáme kurzy...',
      quiz: 'Připravujeme kvíz...',
      login: 'Ověřujeme přihlášení...',
    },
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
    nav: {
      home: 'Domů',
      courses: 'Kurzy',
      about: 'O nás',
      login: 'Přihlásit se',
    },
  },
  en: {
    welcome: 'Welcome to the app',
    loading: {
      default: 'Loading course content...',
      courses: 'Loading courses...',
      quiz: 'Preparing quiz...',
      login: 'Verifying login...',
    },
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
    nav: {
      home: 'Home',
      courses: 'Courses',
      about: 'About',
      login: 'Sign in',
    },
  },
}

export const i18n = createI18n({
  legacy: false,
  locale: 'cs',
  fallbackLocale: 'en',
  messages,
})
