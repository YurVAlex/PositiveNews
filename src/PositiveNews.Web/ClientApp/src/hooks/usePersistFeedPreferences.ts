import { useEffect, useRef } from 'react'
import { putFeedPreferences } from '../api/preferences-api'
import {
  preferencesFromSearchParams,
  serializePreferenceParams,
  snapshotToApiRequest,
} from '../utils/feed-preferences-url'

const SAVE_DEBOUNCE_MS = 500

export function usePersistFeedPreferences(
  searchParams: URLSearchParams,
  token: string | null,
  isAuthenticated: boolean,
  onSaveError: (message: string) => void,
) {
  const lastSavedRef = useRef<string>('')
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    if (!isAuthenticated || !token) {
      return
    }

    const serialized = serializePreferenceParams(searchParams)
    if (serialized === lastSavedRef.current) {
      return
    }

    if (timerRef.current) {
      clearTimeout(timerRef.current)
    }

    timerRef.current = setTimeout(() => {
      const snapshot = preferencesFromSearchParams(searchParams)
      putFeedPreferences(token, snapshotToApiRequest(snapshot))
        .then(() => {
          lastSavedRef.current = serialized
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
  }, [searchParams, token, isAuthenticated, onSaveError])
}
