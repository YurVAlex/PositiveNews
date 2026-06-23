/**
 * Collapsible panel for editing feed topics, sources, and minimum positivity threshold.
 */
import { useCallback, useEffect, useMemo, useState } from 'react'
import { fetchSources } from '../api/sources-api'
import { fetchTopics } from '../api/topics-api'
import type { SourceFilterItem } from '../api/types'
import { DEFAULT_MIN_POSITIVITY } from '../utils/feed-preferences-url'
import { FeedPreferenceMobileSelect } from './FeedPreferenceMobileSelect'

type FeedSettingsPanelProps = {
  selectedTopics: string[]
  selectedSourceIds: number[]
  minPositivity: number
  onTopicsChange: (topics: string[]) => void
  onSourcesChange: (sourceIds: number[]) => void
  onMinPositivityCommit: (value: number) => void
  onClose: () => void
  token: string | null
}

function SettingsPanelHeader({ onClose }: { onClose: () => void }) {
  return (
    <div className="d-flex justify-content-between align-items-start mb-3">
      <h4 className="h6 mb-0">Feed settings</h4>
      <button
        type="button"
        className="btn-close"
        aria-label="Close feed settings"
        onClick={onClose}
      />
    </div>
  )
}

function formatPositivityLabel(value: number): string {
  return `${Math.round(value * 100)}%`
}

export function FeedSettingsPanel({
  selectedTopics,
  selectedSourceIds,
  minPositivity,
  onTopicsChange,
  onSourcesChange,
  onMinPositivityCommit,
  onClose,
  token,
}: FeedSettingsPanelProps) {
  const [allTopics, setAllTopics] = useState<string[]>([])
  const [allSources, setAllSources] = useState<SourceFilterItem[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)
  const [sliderValue, setSliderValue] = useState(minPositivity)

  useEffect(() => {
    setSliderValue(minPositivity)
  }, [minPositivity])

  // Load topic and source catalogs when the panel opens.
  useEffect(() => {
    let cancelled = false
    setLoadError(null)

    ;(async () => {
      try {
        const [topicsRes, sourcesRes] = await Promise.all([
          fetchTopics(token),
          fetchSources(token),
        ])
        if (!cancelled) {
          setAllTopics(topicsRes.topicNames)
          setAllSources(sourcesRes.sources)
        }
      } catch (e) {
        if (!cancelled) {
          setLoadError(e instanceof Error ? e.message : 'Failed to load filter options')
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [token])

  /** Toggles a topic in the feed filter (case-insensitive). */
  const toggleTopic = useCallback(
    (topicName: string) => {
      const trimmed = topicName.trim()
      if (!trimmed.length) return
      const lower = trimmed.toLowerCase()
      const exists = selectedTopics.some((t) => t.toLowerCase() === lower)
      if (exists) {
        onTopicsChange(selectedTopics.filter((t) => t.toLowerCase() !== lower))
      } else {
        onTopicsChange([...selectedTopics, trimmed])
      }
    },
    [selectedTopics, onTopicsChange],
  )

  /** Toggles a source id in the feed filter. */
  const toggleSource = useCallback(
    (sourceId: number) => {
      if (!Number.isInteger(sourceId) || sourceId < 1) return
      const exists = selectedSourceIds.includes(sourceId)
      if (exists) {
        onSourcesChange(selectedSourceIds.filter((id) => id !== sourceId))
      } else {
        onSourcesChange([...selectedSourceIds, sourceId])
      }
    },
    [selectedSourceIds, onSourcesChange],
  )

  /** Pushes the slider value to the parent when the user finishes adjusting it. */
  const commitSlider = useCallback(() => {
    const clamped = Math.min(1, Math.max(0, sliderValue))
    onMinPositivityCommit(clamped)
  }, [sliderValue, onMinPositivityCommit])

  const topicOptions = useMemo(() => {
    const fromCatalog = allTopics.map((topic) => ({ value: topic, label: topic }))
    const known = new Set(fromCatalog.map((o) => o.value.toLowerCase()))
    const extra = selectedTopics
      .filter((t) => !known.has(t.toLowerCase()))
      .map((t) => ({ value: t, label: t }))
    return [...extra, ...fromCatalog]
  }, [allTopics, selectedTopics])

  const sourceOptions = useMemo(() => {
    const fromCatalog = allSources.map((source) => ({ value: source.id, label: source.name }))
    const known = new Set(fromCatalog.map((o) => o.value))
    const extra = selectedSourceIds
      .filter((id) => !known.has(id))
      .map((id) => ({ value: id, label: `Source #${id}` }))
    return [...extra, ...fromCatalog]
  }, [allSources, selectedSourceIds])

  if (loadError) {
    return (
      <section className="feed-settings-panel card card-body shadow-sm mb-3" aria-label="Feed settings">
        <SettingsPanelHeader onClose={onClose} />
        <div className="alert alert-warning mb-0 py-2">{loadError}</div>
      </section>
    )
  }

  return (
    <section className="feed-settings-panel card card-body shadow-sm mb-3" aria-label="Feed settings">
      <SettingsPanelHeader onClose={onClose} />

      <div className="mb-3">
        <label className="form-label small fw-semibold mb-2">Topics</label>
        <div className="d-none d-lg-flex flex-wrap gap-2">
          {allTopics.length === 0 ? (
            <span className="text-muted small">No topics available</span>
          ) : (
            allTopics.map((topic) => {
              const selected = selectedTopics.some((t) => t.toLowerCase() === topic.toLowerCase())
              return (
                <button
                  key={topic}
                  type="button"
                  className={`btn btn-sm ${selected ? 'btn-primary' : 'btn-outline-secondary'}`}
                  aria-pressed={selected}
                  onClick={() => toggleTopic(topic)}
                >
                  {topic}
                </button>
              )
            })
          )}
        </div>
        <div className="d-lg-none">
          {allTopics.length === 0 && topicOptions.length === 0 ? (
            <span className="text-muted small">No topics available</span>
          ) : (
            <FeedPreferenceMobileSelect
              ariaLabel="Select topics"
              options={topicOptions}
              selectedValues={selectedTopics}
              onChange={onTopicsChange}
              equals={(a, b) => a.toLowerCase() === b.toLowerCase()}
            />
          )}
        </div>
      </div>

      <div className="mb-3">
        <label className="form-label small fw-semibold mb-2">Sources</label>
        <div className="d-none d-lg-flex flex-wrap gap-2">
          {allSources.length === 0 ? (
            <span className="text-muted small">No sources available</span>
          ) : (
            allSources.map((source) => {
              const selected = selectedSourceIds.includes(source.id)
              return (
                <button
                  key={source.id}
                  type="button"
                  className={`btn btn-sm d-inline-flex align-items-center gap-1 ${selected ? 'btn-primary' : 'btn-outline-secondary'}`}
                  aria-pressed={selected}
                  onClick={() => toggleSource(source.id)}
                >
                  {source.logoUrl ? (
                    <img src={source.logoUrl} alt="" width={16} height={16} style={{ objectFit: 'cover' }} />
                  ) : null}
                  {source.name}
                </button>
              )
            })
          )}
        </div>
        <div className="d-lg-none">
          {allSources.length === 0 && sourceOptions.length === 0 ? (
            <span className="text-muted small">No sources available</span>
          ) : (
            <FeedPreferenceMobileSelect
              ariaLabel="Select sources"
              options={sourceOptions}
              selectedValues={selectedSourceIds}
              onChange={onSourcesChange}
            />
          )}
        </div>
      </div>

      <div>
        <label htmlFor="feed-min-positivity" className="form-label small fw-semibold mb-2">
          Minimum positivity: {formatPositivityLabel(sliderValue)}
        </label>
        <input
          id="feed-min-positivity"
          type="range"
          className="form-range"
          min={0}
          max={100}
          step={1}
          value={Math.round(sliderValue * 100)}
          onChange={(e) => setSliderValue(Number(e.target.value) / 100)}
          onMouseUp={commitSlider}
          onTouchEnd={commitSlider}
          onKeyUp={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              commitSlider()
            }
          }}
        />
        <div className="d-flex justify-content-between small text-muted">
          <span>0%</span>
          <span className="text-center">Default {formatPositivityLabel(DEFAULT_MIN_POSITIVITY)}</span>
          <span>100%</span>
        </div>
      </div>
    </section>
  )
}
