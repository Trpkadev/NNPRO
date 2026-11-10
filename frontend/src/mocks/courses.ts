export interface MockCourse {
  id: number
  titleKey: string
  descriptionKey: string
  lessonKeys: string[]
}

export const mockCourses: MockCourse[] = [
  {
    id: 1,
    titleKey: 'courses.mock.csharp.title',
    descriptionKey: 'courses.mock.csharp.description',
    lessonKeys: [
      'courses.mock.csharp.lessons.intro',
      'courses.mock.csharp.lessons.variables',
      'courses.mock.csharp.lessons.conditions',
      'courses.mock.csharp.lessons.methods',
      'courses.mock.csharp.lessons.objects',
    ],
  },
  {
    id: 2,
    titleKey: 'courses.mock.aspnet.title',
    descriptionKey: 'courses.mock.aspnet.description',
    lessonKeys: [
      'courses.mock.aspnet.lessons.intro',
      'courses.mock.aspnet.lessons.controllers',
      'courses.mock.aspnet.lessons.dto',
      'courses.mock.aspnet.lessons.database',
    ],
  },
  {
    id: 3,
    titleKey: 'courses.mock.sql.title',
    descriptionKey: 'courses.mock.sql.description',
    lessonKeys: [
      'courses.mock.sql.lessons.intro',
      'courses.mock.sql.lessons.queries',
      'courses.mock.sql.lessons.joins',
      'courses.mock.sql.lessons.indexes',
    ],
  },
  {
    id: 4,
    titleKey: 'courses.mock.react.title',
    descriptionKey: 'courses.mock.react.description',
    lessonKeys: [
      'courses.mock.react.lessons.intro',
      'courses.mock.react.lessons.components',
      'courses.mock.react.lessons.state',
      'courses.mock.react.lessons.routing',
    ],
  },
  {
    id: 5,
    titleKey: 'courses.mock.git.title',
    descriptionKey: 'courses.mock.git.description',
    lessonKeys: [
      'courses.mock.git.lessons.intro',
      'courses.mock.git.lessons.branches',
      'courses.mock.git.lessons.conflicts',
      'courses.mock.git.lessons.pullRequests',
    ],
  },
  {
    id: 6,
    titleKey: 'courses.mock.security.title',
    descriptionKey: 'courses.mock.security.description',
    lessonKeys: [
      'courses.mock.security.lessons.threats',
      'courses.mock.security.lessons.passwords',
      'courses.mock.security.lessons.authentication',
      'courses.mock.security.lessons.web',
    ],
  },
  {
    id: 7,
    titleKey: 'courses.mock.algorithms.title',
    descriptionKey: 'courses.mock.algorithms.description',
    lessonKeys: [
      'courses.mock.algorithms.lessons.complexity',
      'courses.mock.algorithms.lessons.sorting',
      'courses.mock.algorithms.lessons.recursion',
      'courses.mock.algorithms.lessons.graphs',
    ],
  },
  {
    id: 8,
    titleKey: 'courses.mock.testing.title',
    descriptionKey: 'courses.mock.testing.description',
    lessonKeys: [
      'courses.mock.testing.lessons.intro',
      'courses.mock.testing.lessons.unit',
      'courses.mock.testing.lessons.mocking',
      'courses.mock.testing.lessons.integration',
    ],
  },
]
