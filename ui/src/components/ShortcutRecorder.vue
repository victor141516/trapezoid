<script setup lang="ts">
import { ref, watch } from "vue";

export interface Shortcut {
  ALT: boolean;
  CTRL: boolean;
  SHIFT: boolean;
  META: boolean;
  KEY: string;
}

const inputValue = ref<null | Shortcut>(null);

const props = defineProps<{
  modelValue: Shortcut | null;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", value: Shortcut | null): void;
}>();

watch(
  () => props.modelValue,
  () => {
    inputValue.value = props.modelValue;
  },
  { immediate: true }
);

const KEY_REPRESENTATION = {
  ArrowUp: "↑",
  ArrowDown: "↓",
  ArrowLeft: "←",
  ArrowRight: "→",
} as const;

const getShortcutRepresentation = (shortcut: Shortcut | null) => {
  if (shortcut === null) return "";
  const representation = [
    shortcut.ALT ? "Alt" : null,
    shortcut.CTRL ? "Ctrl" : null,
    shortcut.SHIFT ? "Shift" : null,
    shortcut.META ? "Win" : null,
    shortcut.KEY,
  ]
    .filter((e) => e !== null)
    .join("+");
  return representation;
};

const onKeydown = (event: KeyboardEvent) => {
  const keyCode = event.code;
  const onlyModifierKeys = [
    "ControlLeft",
    "ControlRight",
    "ShiftLeft",
    "ShiftRight",
    "AltLeft",
    "AltRight",
    "MetaLeft",
    "MetaRight",
  ].includes(keyCode);

  let deleteShortcut = false;
  let key: string | null = keyCode;
  if (onlyModifierKeys) {
    key = null;
  } else {
    key = key.replace(/(^Key)|(^Digit)/g, "");
    key = KEY_REPRESENTATION[key as keyof typeof KEY_REPRESENTATION] ?? key;
    if (key === "Backspace") {
      deleteShortcut = true;
    }
  }

  let value: Shortcut | null = {
    ALT: event.altKey,
    CTRL: event.ctrlKey,
    SHIFT: event.shiftKey,
    META: event.metaKey,
    // display the partial shortcut but don't emit it
    KEY: key ?? "",
  };

  console.log({ deleteShortcut });
  if (deleteShortcut) {
    value = null;
  }

  inputValue.value = value;

  if (key !== null) {
    emit("update:modelValue", value);
  }
};
</script>

<template>
  <input
    @keydown.prevent.stop="(e) => onKeydown(e)"
    class="px-1 py-0.5 border"
    type="text"
    :value="getShortcutRepresentation(inputValue)"
  />
</template>
