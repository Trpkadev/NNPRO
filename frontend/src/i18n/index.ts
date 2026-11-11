import { createI18n } from 'vue-i18n'
import { watch } from 'vue'

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
      backToCatalog: 'Zpět na katalog',
      startLearning: 'Začít studovat',
      courseContent: 'Obsah kurzu',
      lessons: 'lekcí',
      onlineCourse: 'Online kurz',
      course: 'Kurz',
      lesson: 'Lekce',
      sampleLesson: 'Ukázková lekce',
      lessonPlaceholder: 'Zde se bude zobrazovat obsah vybrané lekce.',
      courseNotFound: 'Kurz nebyl nalezen.',

      // Průběh studia
      sectionProgress: 'Průběh studia v sekci',
      completedCount: 'Dokončeno {count} z {total} {lessonForm}',
      markCompleted: 'Dokončit',
      completed: 'Dokončeno',
      progressError: 'Nepodařilo se aktualizovat dokončení lekce.',

      previousLesson: 'Předchozí lekce',
      nextLesson: 'Další lekce',
      lessonPosition: 'Lekce {current} z {total}',
      searchPlaceholder: 'Hledat kurz...',
      clearSearch: 'Vymazat hledání',
      noSearchResults: 'Žádné kurzy neodpovídají hledání.',
      filterBy: 'Filtrovat podle:',
      filter: {
        all: 'Vše',
        title: 'Název',
        description: 'Popis',
      },

      mock: {
        csharp: {
          title: 'Základy programování v C#',
          description:
            'Praktický úvod do proměnných, podmínek, cyklů, metod a objektově orientovaného programování v jazyce C#.',
          lessons: {
            intro: 'Úvod do C#',
            variables: 'Proměnné a datové typy',
            conditions: 'Podmínky a cykly',
            methods: 'Metody a funkce',
            objects: 'Třídy a objekty',
          },
        },
        aspnet: {
          title: 'Vývoj webových API v ASP.NET Core',
          description:
            'Naučíte se navrhovat REST API, pracovat s controllery, DTO objekty, validací a databází pomocí Entity Framework Core.',
          lessons: {
            intro: 'Úvod do REST API',
            controllers: 'Controllery',
            dto: 'DTO objekty a validace',
            database: 'Entity Framework Core',
          },
        },
        sql: {
          title: 'Databáze a SQL pro vývojáře',
          description:
            'Kurz pokrývá návrh relačních databází, SQL dotazy, vazby mezi tabulkami, indexy a základní optimalizaci.',
          lessons: {
            intro: 'Relační databáze',
            queries: 'SQL dotazy',
            joins: 'Spojování tabulek',
            indexes: 'Indexy a optimalizace',
          },
        },
        react: {
          title: 'Frontend s Reactem a TypeScriptem',
          description:
            'Vytvořte moderní webovou aplikaci s Reactem, komponentami, stavem, routováním a typovou kontrolou v TypeScriptu.',
          lessons: {
            intro: 'Úvod do Reactu',
            components: 'Komponenty a Props',
            state: 'Správa stavu',
            routing: 'Routování a TypeScript',
          },
        },
        git: {
          title: 'Git a týmová spolupráce',
          description:
            'Od základních příkazů Gitu až po práci s větvemi, pull requesty, řešením konfliktů a code review.',
          lessons: {
            intro: 'Základy Gitu',
            branches: 'Práce s větvemi',
            conflicts: 'Řešení merge konfliktů',
            pullRequests: 'Pull requesty a code review',
          },
        },
        security: {
          title: 'Úvod do kybernetické bezpečnosti',
          description:
            'Seznamte se s nejčastějšími hrozbami, bezpečným ukládáním hesel, autentizací a ochranou webových aplikací.',
          lessons: {
            threats: 'Bezpečnostní hrozby',
            passwords: 'Bezpečná hesla',
            authentication: 'Autentizace',
            web: 'Bezpečnost webových aplikací',
          },
        },
        algorithms: {
          title: 'Algoritmy a datové struktury',
          description:
            'Pochopte principy třídění, vyhledávání, rekurze, stromů a grafů s důrazem na časovou a paměťovou složitost.',
          lessons: {
            complexity: 'Časová a paměťová složitost',
            sorting: 'Třídění a vyhledávání',
            recursion: 'Rekurze',
            graphs: 'Stromy a grafy',
          },
        },
        testing: {
          title: 'Automatizované testování v .NET',
          description:
            'Praktický kurz unit testů, integračních testů, mockování závislostí a testování webových endpointů.',
          lessons: {
            intro: 'Úvod do testování',
            unit: 'Unit testy',
            mocking: 'Mockování závislostí',
            integration: 'Integrační testy',
          },
        },
      },
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
      backToCatalog: 'Back to Catalog',
      startLearning: 'Start Learning',
      courseContent: 'Course Content',
      lessons: 'lessons',
      onlineCourse: 'Online Course',
      course: 'Course',
      lesson: 'Lesson',
      sampleLesson: 'Sample Lesson',
      lessonPlaceholder: 'The content of the selected lesson will be displayed here.',
      courseNotFound: 'Course not found.',

      previousLesson: 'Previous Lesson',
      nextLesson: 'Next Lesson',
      lessonPosition: 'Lesson {current} of {total}',
      searchPlaceholder: 'Search courses...',
      clearSearch: 'Clear search',
      noSearchResults: 'No courses match your search.',
      filterBy: 'Filter by:',
      filter: {
        all: 'All',
        title: 'Title',
        description: 'Description',
      },

      // Study progress
      sectionProgress: 'Section progress',
      completedCount: '{count} of {total} {lessonForm} completed',
      markCompleted: 'Complete',
      completed: 'Completed',
      progressError: 'Failed to update lesson progress.',

      mock: {
        csharp: {
          title: 'C# Programming Fundamentals',
          description:
            'A practical introduction to variables, conditions, loops, methods and object-oriented programming in C#.',
          lessons: {
            intro: 'Introduction to C#',
            variables: 'Variables and Data Types',
            conditions: 'Conditions and Loops',
            methods: 'Methods and Functions',
            objects: 'Classes and Objects',
          },
        },
        aspnet: {
          title: 'Building Web APIs with ASP.NET Core',
          description:
            'Learn to design REST APIs and work with controllers, DTOs, validation and databases using Entity Framework Core.',
          lessons: {
            intro: 'Introduction to REST APIs',
            controllers: 'Controllers',
            dto: 'DTOs and Validation',
            database: 'Entity Framework Core',
          },
        },
        sql: {
          title: 'Databases and SQL for Developers',
          description:
            'Explore relational database design, SQL queries, table relationships, indexes and basic optimization.',
          lessons: {
            intro: 'Relational Databases',
            queries: 'SQL Queries',
            joins: 'Table Joins',
            indexes: 'Indexes and Optimization',
          },
        },
        react: {
          title: 'Frontend with React and TypeScript',
          description:
            'Build a modern web application with React, components, state management, routing and TypeScript.',
          lessons: {
            intro: 'Introduction to React',
            components: 'Components and Props',
            state: 'State Management',
            routing: 'Routing and TypeScript',
          },
        },
        git: {
          title: 'Git and Team Collaboration',
          description:
            'Learn Git commands, branching, pull requests, merge conflict resolution and code reviews.',
          lessons: {
            intro: 'Git Basics',
            branches: 'Working with Branches',
            conflicts: 'Resolving Merge Conflicts',
            pullRequests: 'Pull Requests and Code Review',
          },
        },
        security: {
          title: 'Introduction to Cybersecurity',
          description:
            'Discover common security threats, secure password storage, authentication and web application protection.',
          lessons: {
            threats: 'Security Threats',
            passwords: 'Secure Passwords',
            authentication: 'Authentication',
            web: 'Web Application Security',
          },
        },
        algorithms: {
          title: 'Algorithms and Data Structures',
          description:
            'Understand sorting, searching, recursion, trees and graphs with a focus on time and space complexity.',
          lessons: {
            complexity: 'Time and Space Complexity',
            sorting: 'Sorting and Searching',
            recursion: 'Recursion',
            graphs: 'Trees and Graphs',
          },
        },
        testing: {
          title: 'Automated Testing in .NET',
          description:
            'A practical course on unit tests, integration tests, dependency mocking and testing web endpoints.',
          lessons: {
            intro: 'Introduction to Testing',
            unit: 'Unit Tests',
            mocking: 'Dependency Mocking',
            integration: 'Integration Tests',
          },
        },
      },
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

watch(
  i18n.global.locale,
  (lang) => {
    document.documentElement.lang = lang
  },
  { immediate: true },
)