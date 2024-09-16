<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue";
import ShortcutRecorder, { Shortcut } from "./components/ShortcutRecorder.vue";
import * as api from "./libs/api";

const fromActionListToInputValues = (actions: api.AvailableActions[]) => {
  return actions.reduce((acc, e) => {
    return { ...acc, [e]: null };
  }, {}) as Record<(typeof availableActions.value)[number], null | Shortcut>;
};

const inputEl = ref<HTMLInputElement>();
const isEnabled = ref(false);
const availableActions = ref<api.AvailableActions[]>([]);
const inputValues = reactive(
  fromActionListToInputValues(availableActions.value)
);
const offsets = reactive({
  top: 0,
  left: 0,
  bottom: 0,
  right: 0,
});

watch(isEnabled, async () => {
  let action!: () => Promise<{ ok: boolean }>;
  if (isEnabled.value) {
    action = api.pause;
  } else {
    action = api.resume;
  }

  await action()
    .then(() => {
      if (inputEl.value) {
        inputEl.value.disabled = true;
      }
      isEnabled.value = !isEnabled.value;
    })
    .catch((e) => {
      console.error(e);
    });

  if (inputEl.value) {
    inputEl.value.disabled = false;
  }
});

watch(offsets, async () => {
  if (!isInitialized.value) return;
  await api.setOffsets(offsets);
});

watch(
  inputValues,
  () => {
    if (!isInitialized.value) return;
    console.log("setting shortcuts actions");
    api.setShortcutsActions(
      Object.entries(inputValues)
        .map(([k, v]) => {
          return {
            shortcut: v as Shortcut,
            action: k as api.AvailableActions,
          };
        })
        .filter((e) => e.shortcut !== null)
    );
  },
  { immediate: false, deep: true }
);

const fetchOffsets = async () => {
  await api.getOffsets().then(({ top, left, bottom, right }) => {
    offsets.top = top;
    offsets.left = left;
    offsets.bottom = bottom;
    offsets.right = right;
  });
};

const isInitialized = ref(false);
onMounted(async () => {
  api.setApiPort(10398);
  api.setApiPort(8000);
  await api.resume();
  await api.getAvailableActions().then((actions) => {
    availableActions.value = actions;
    Object.entries(fromActionListToInputValues(actions)).forEach(([k, v]) => {
      inputValues[k as api.AvailableActions] = v;
    });
  });
  await api.getShortcutsActions().then((shortcutsActions) => {
    shortcutsActions.forEach(({ action, shortcut }) => {
      inputValues[action] = shortcut;
    });
  });
  await fetchOffsets();

  isInitialized.value = true;
});
</script>

<template>
  <div v-if="isInitialized" class="flex flex-col gap-4">
    <label class="flex gap-2">
      <span>Enabled</span>
      <input ref="inputEl" type="checkbox" :value="isEnabled" />
    </label>
    <div>
      <fieldset class="flex gap-4">
        <legend>Offsets</legend>
        <label class="flex gap-2">
          <span>Top</span>
          <input
            class="px-1 py-0.5 border text-right max-w-24"
            type="number"
            step="1"
            v-model="offsets.top"
          />
        </label>
        <label class="flex gap-2">
          <span>Left</span>
          <input
            class="px-1 py-0.5 border text-right max-w-24"
            type="number"
            step="1"
            v-model="offsets.left"
          />
        </label>
        <label class="flex gap-2">
          <span>Bottom</span>
          <input
            class="px-1 py-0.5 border text-right max-w-24"
            type="number"
            step="1"
            v-model="offsets.bottom"
          />
        </label>
        <label class="flex gap-2">
          <span>Right</span>
          <input
            class="px-1 py-0.5 border text-right max-w-24"
            type="number"
            step="1"
            v-model="offsets.right"
          />
        </label>
      </fieldset>
    </div>
    <fieldset class="flex flex-col gap-2">
      <legend>Shortcuts</legend>
      <div
        v-for="action of availableActions"
        :key="action"
        class="flex items-center"
      >
        <label class="flex gap-2">
          <span>{{ action }}</span>
          <ShortcutRecorder v-model="inputValues[action]" />
        </label>
      </div>
    </fieldset>
  </div>
</template>
