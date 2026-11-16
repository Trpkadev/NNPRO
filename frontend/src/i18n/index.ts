import { createI18n } from 'vue-i18n'

export const LOCALE_STORAGE_KEY = 'app_locale'
export type SupportedLocale = 'cs' | 'en'

function getInitialLocale(): SupportedLocale {
  if (typeof window !== 'undefined' && window.localStorage) {
    const saved = window.localStorage.getItem(LOCALE_STORAGE_KEY)
    if (saved === 'cs' || saved === 'en') {
      return saved
    }
  }
  return 'cs'
}

const messages = {
  cs: {
    app: {
      name: 'E-Learning',
      subtitle: 'Vzdělávací platforma',
    },
    welcome: 'Vítejte v aplikaci',
    nav: {
      home: 'Přehled',
      courses: 'Katalog kurzů',
      instructorZone: 'Lektorská zóna',
      adminZone: 'Administrace',
      usersManagement: 'Správa uživatelů',
      coursesManagement: 'Správa kurzů',
      login: 'Přihlásit se',
      register: 'Registrace',
      logout: 'Odhlásit se',
      darkMode: 'Přepnout vzhled',
      language: 'Jazyk',
    },
    roles: {
      Admin: 'Správce',
      Instructor: 'Lektor',
      Student: 'Student',
      guest: 'Host',
    },
    auth: {
      loginTitle: 'Přihlášení do účtu',
      loginSubtitle: 'Zadejte své přihlašovací údaje pro přístup ke kurzům',
      registerTitle: 'Vytvořit nový účet',
      registerSubtitle: 'Zaregistrujte se a začněte studovat kurzy ještě dnes',
      identifierLabel: 'E-mail nebo uživatelské jméno',
      identifierPlaceholder: "např. jan.novak{'@'}example.com nebo jannovak",
      usernameLabel: 'Uživatelské jméno',
      usernamePlaceholder: 'Zadejte uživatelské jméno',
      emailLabel: 'E-mailová adresa',
      emailPlaceholder: "jan.novak{'@'}example.com",
      passwordLabel: 'Heslo',
      passwordPlaceholder: 'Zadejte heslo',
      confirmPasswordLabel: 'Potvrzení hesla',
      confirmPasswordPlaceholder: 'Zadejte heslo znovu',
      loginSubmit: 'Přihlásit se',
      registerSubmit: 'Zaregistrovat se',
      noAccount: 'Ještě nemáte účet?',
      createAccountLink: 'Zaregistrujte se',
      haveAccount: 'Již máte účet?',
      loginLink: 'Přihlaste se',
      loginSuccess: 'Přihlášení proběhlo úspěšně',
      registerSuccess: 'Registrace proběhla úspěšně',
      logoutSuccess: 'Byli jste úspěšně odhlášeni',
      logoutError: 'Odhlášení na serveru se nezdařilo, lokální relace byla ukončena',
      loggedInAs: 'Přihlášen jako',
      loginError: 'Neplatné přihlašovací údaje',
      registerError: 'Registrace se nezdařila',
      registerConflictError: 'Uživatel s tímto jménem nebo e-mailem již existuje',
    },
    notFound: {
      title: 'Stránka nenalezena',
      description: 'Omlouváme se, ale stránka, kterou hledáte, neexistuje nebo byla přesunuta.',
      goBack: 'Zpět',
      goHome: 'Domů',
    },
    forbidden: {
      title: 'Přístup odepřen',
      description: 'Nemáte dostatečná oprávnění pro přístup k této sekci aplikace.',
      goBack: 'Zpět',
      goHome: 'Domů',
    },
    validation: {
      required: 'Toto pole je povinné',
      email: 'Zadejte platný e-mail',
      minLength: 'Minimální délka je {min} znaků',
      usernameNoAt: "Uživatelské jméno nesmí obsahovat znak {'@'}",
      passwordsDontMatch: 'Hesla se neshodují',
    },
    courses: {
      empty: 'Žádné kurzy k zobrazení.',
      loadError: 'Kurzy se nepodařilo načíst.',
    },
    admin: {
      id: 'ID',
      username: 'Uživatelské jméno',
      email: 'E-mail',
      role: 'Role',
      usersLoadError: 'Uživatele se nepodařilo načíst.',
    },
  },
  en: {
    app: {
      name: 'E-Learning',
      subtitle: 'Learning Platform',
    },
    welcome: 'Welcome to the app',
    nav: {
      home: 'Dashboard',
      courses: 'Course Catalog',
      instructorZone: 'Instructor Zone',
      adminZone: 'Administration',
      usersManagement: 'User Management',
      coursesManagement: 'Course Management',
      login: 'Sign In',
      register: 'Sign Up',
      logout: 'Sign Out',
      darkMode: 'Toggle theme',
      language: 'Language',
    },
    roles: {
      Admin: 'Administrator',
      Instructor: 'Instructor',
      Student: 'Student',
      guest: 'Guest',
    },
    auth: {
      loginTitle: 'Sign in to your account',
      loginSubtitle: 'Enter your credentials to access your courses',
      registerTitle: 'Create a new account',
      registerSubtitle: 'Sign up and start learning today',
      identifierLabel: 'Email or username',
      identifierPlaceholder: "e.g. john.doe{'@'}example.com or johndoe",
      usernameLabel: 'Username',
      usernamePlaceholder: 'Enter username',
      emailLabel: 'Email address',
      emailPlaceholder: "john.doe{'@'}example.com",
      passwordLabel: 'Password',
      passwordPlaceholder: 'Enter password',
      confirmPasswordLabel: 'Confirm password',
      confirmPasswordPlaceholder: 'Re-enter password',
      loginSubmit: 'Sign In',
      registerSubmit: 'Sign Up',
      noAccount: "Don't have an account yet?",
      createAccountLink: 'Create an account',
      haveAccount: 'Already have an account?',
      loginLink: 'Sign in',
      loginSuccess: 'Successfully signed in',
      registerSuccess: 'Account created successfully',
      logoutSuccess: 'You have been signed out',
      logoutError: 'Server sign-out failed, but your local session was ended',
      loggedInAs: 'Signed in as',
      loginError: 'Invalid credentials',
      registerError: 'Registration failed',
      registerConflictError: 'A user with this username or email already exists',
    },
    notFound: {
      title: 'Page not found',
      description: 'Sorry, the page you are looking for does not exist or has been moved.',
      goBack: 'Go back',
      goHome: 'Home',
    },
    forbidden: {
      title: 'Access Denied',
      description: 'You do not have sufficient permissions to access this section.',
      goBack: 'Go back',
      goHome: 'Home',
    },
    validation: {
      required: 'This field is required',
      email: 'Enter a valid email',
      minLength: 'Minimum length is {min} characters',
      usernameNoAt: "Username cannot contain the {'@'} character",
      passwordsDontMatch: 'Passwords do not match',
    },
    courses: {
      empty: 'No courses to display.',
      loadError: 'Courses could not be loaded.',
    },
    admin: {
      id: 'ID',
      username: 'Username',
      email: 'Email',
      role: 'Role',
      usersLoadError: 'Users could not be loaded.',
    },
  },
}

export const i18n = createI18n({
  legacy: false, // Nutné pro Composition API
  locale: getInitialLocale(),
  fallbackLocale: 'en',
  messages,
})
