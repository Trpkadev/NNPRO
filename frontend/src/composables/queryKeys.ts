import type { CourseThumbnailQueryParameters, QueryParameters } from '@/types/api'

export const queryKeys = {
  courses: {
    all: ['courses'] as const,
    lists: () => [...queryKeys.courses.all, 'list'] as const,
    list: (params?: QueryParameters) => [...queryKeys.courses.lists(), params ?? {}] as const,
    thumbnails: () => [...queryKeys.courses.all, 'thumbnails'] as const,
    thumbnailList: (params?: CourseThumbnailQueryParameters) =>
      [...queryKeys.courses.thumbnails(), params ?? {}] as const,
    details: () => [...queryKeys.courses.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.courses.details(), id] as const,
  },
  enrollments: {
    all: ['enrollments'] as const,
    byCourse: (courseId: number, params?: QueryParameters) =>
      [...queryKeys.enrollments.all, 'course', courseId, params ?? {}] as const,
  },
  sections: {
    all: ['sections'] as const,
    details: () => [...queryKeys.sections.all, 'detail'] as const,
    detail: (sectionId: number) => [...queryKeys.sections.details(), sectionId] as const,
  },
  lessons: {
    all: ['lessons'] as const,
    details: () => [...queryKeys.lessons.all, 'detail'] as const,
    detail: (lessonId: number) => [...queryKeys.lessons.details(), lessonId] as const,
  },
  quizzes: {
    all: ['quizzes'] as const,
    details: () => [...queryKeys.quizzes.all, 'detail'] as const,
    detail: (quizId: number) => [...queryKeys.quizzes.details(), quizId] as const,
    editor: (quizId: number) => [...queryKeys.quizzes.all, 'editor', quizId] as const,
    attempts: (quizId: number, params?: QueryParameters) =>
      [...queryKeys.quizzes.all, quizId, 'attempts', params ?? {}] as const,
    attemptDetail: (attemptId: number) => [...queryKeys.quizzes.all, 'attempt', attemptId] as const,
  },
  questions: {
    all: ['questions'] as const,
    details: () => [...queryKeys.questions.all, 'detail'] as const,
    detail: (questionId: number) => [...queryKeys.questions.details(), questionId] as const,
  },
  users: {
    all: ['users'] as const,
    lists: () => [...queryKeys.users.all, 'list'] as const,
    list: (params?: QueryParameters) => [...queryKeys.users.lists(), params ?? {}] as const,
    details: () => [...queryKeys.users.all, 'detail'] as const,
    detail: (userId: number) => [...queryKeys.users.details(), userId] as const,
  },
}
