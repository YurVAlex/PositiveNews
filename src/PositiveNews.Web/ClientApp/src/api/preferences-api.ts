/** Persisted feed filter and sort preferences for authenticated users. */

import type { UserFeedPreferencesResponse } from './types'
import { apiUrl, authTokenHeader } from './http'

/** Loads the user's saved feed filters so the UI can restore them across sessions. */
export async function getFeedPreferences(token: string): Promise<UserFeedPreferencesResponse> {
  const res = await fetch(apiUrl('/api/users/me/feed-preferences'), {
    headers: authTokenHeader(token),
  })

  if (!res.ok) {
    throw new Error(`Failed to load feed preferences (${res.status})`)
  }

  return res.json() as Promise<UserFeedPreferencesResponse>
}

/** Replaces feed preferences on the server; returns the canonical saved snapshot. */
export async function putFeedPreferences(
  token: string,
  snapshot: UserFeedPreferencesResponse,
): Promise<UserFeedPreferencesResponse> {
  const res = await fetch(apiUrl('/api/users/me/feed-preferences'), {
    method: 'PUT',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      topicNames: snapshot.topicNames,
      sourceIds: snapshot.sourceIds,
      minPositivity: snapshot.minPositivity,
      sortBy: snapshot.sortBy,
    }),
  })

  if (!res.ok) {
    throw new Error(`Failed to save feed preferences (${res.status})`)
  }

  return res.json() as Promise<UserFeedPreferencesResponse>
}
