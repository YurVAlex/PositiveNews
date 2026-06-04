import { useEffect, useRef } from 'react'
import { putFeedPreferences } from '../api/preferences-api'
import {
  loadLastSavedPreferenceParams,
  preferencesFromSearchParams,
  saveLastSavedPreferenceParams,
  serializePreferenceParams,
  shouldHydrateFeedFromDraft,
  snapshotToApiRequest,
} from '../utils/feed-preferences-url'

const SAVE_DEBOUNCE_MS = 500

export function usePersistFeedPreferences(
  searchParams: URLSearchParams,
  token: string | null,
  isAuthenticated: boolean,
  userId: number | null,
  onSaveError: (message: string) => void,
) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    if (!isAuthenticated || !token || userId == null) {
      return
    }

    if (shouldHydrateFeedFromDraft(searchParams)) {
      return
    }

    const serialized = serializePreferenceParams(searchParams)
    if (serialized === loadLastSavedPreferenceParams(userId)) {
      return
    }

    if (timerRef.current) {
      clearTimeout(timerRef.current)
    }

    timerRef.current = setTimeout(() => {
      const snapshot = preferencesFromSearchParams(searchParams)
      putFeedPreferences(token, snapshotToApiRequest(snapshot))
        .then(() => {
          saveLastSavedPreferenceParams(userId, serialized)
        })
        .catch((e) => {
          onSaveError(e instanceof Error ? e.message : 'Failed to save preferences')
        })
    }, SAVE_DEBOUNCE_MS)

    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current)
      }
    }
  }, [searchParams, token, isAuthenticated, userId, onSaveError])
}
