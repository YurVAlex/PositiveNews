import { useCallback } from 'react'

type FeedPreferenceMobileSelectProps<T extends string | number> = {
  ariaLabel: string
  options: { value: T; label: string }[]
  selectedValues: T[]
  onChange: (values: T[]) => void
  equals?: (a: T, b: T) => boolean
}

function defaultEquals<T>(a: T, b: T): boolean {
  return a === b
}

function formatActiveOptionLabel(label: string): string {
  return `${label} ×`
}

export function FeedPreferenceMobileSelect<T extends string | number>({
  ariaLabel,
  options,
  selectedValues,
  onChange,
  equals = defaultEquals,
}: FeedPreferenceMobileSelectProps<T>) {
  const isSelected = useCallback(
    (value: T) => selectedValues.some((v) => equals(v, value)),
    [selectedValues, equals],
  )

  const removeValue = useCallback(
    (value: T) => {
      onChange(selectedValues.filter((v) => !equals(v, value)))
    },
    [selectedValues, onChange, equals],
  )

  const toggleValue = useCallback(
    (value: T) => {
      if (isSelected(value)) {
        removeValue(value)
      } else {
        onChange([...selectedValues, value])
      }
    },
    [isSelected, removeValue, onChange, selectedValues],
  )

  const listSize = Math.min(6, Math.max(3, options.length))
  const listHeightEm = listSize * 1.75

  return (
    <div
      className="form-select feed-preference-select feed-preference-listbox p-0 overflow-auto"
      role="listbox"
      aria-label={ariaLabel}
      aria-multiselectable="true"
      style={{ height: `${listHeightEm}em` }}
    >
      {options.map((option) => {
        const selected = isSelected(option.value)
        const label = selected ? formatActiveOptionLabel(option.label) : option.label
        return (
          <button
            key={String(option.value)}
            type="button"
            role="option"
            aria-selected={selected}
            className={[
              'feed-preference-listbox__option',
              selected ? 'feed-preference-select__option--active' : '',
            ]
              .filter(Boolean)
              .join(' ')}
            onClick={() => toggleValue(option.value)}
          >
            {label}
          </button>
        )
      })}
    </div>
  )
}
