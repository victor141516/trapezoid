<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue";
import ShortcutRecorder, { Shortcut } from "./components/ShortcutRecorder.vue";
import Toggle from "./components/Toggle.vue";
import * as api from "./libs/api";
import { Action, Offsets } from "@trapezoid/common/window";
import { ShortcutAction } from "@trapezoid/common/shortcuts";
import WindowIcon from "./components/WindowIcon.vue";

const fromActionListToInputValues = (actions: Action[]) => {
  return actions.reduce((acc, e) => {
    return { ...acc, [e]: null };
  }, {}) as Record<(typeof availableActions.value)[number], null | Shortcut>;
};

const isEnabled = ref(false);
const availableActions = ref<Action[]>([]);
const inputValues = reactive(
  fromActionListToInputValues(availableActions.value)
);
const offsets = reactive<Offsets>({
  top: 0,
  left: 0,
  bottom: 0,
  right: 0,
});

const translateActions: Record<Action, string> = {
  fullScreen: "Full screen",
  leftOneThird: "Left one third",
  leftTwoThirds: "Left two thirds",
  rightOneThird: "Right one third",
  rightTwoThirds: "Right two thirds",
  leftHalf: "Left half",
  dynamicLeftHalf: "Dynamic left half",
  dynamicRightHalf: "Dynamic right half",
  rightHalf: "Right half",
  topLeftSixth: "Top left sixth",
  topMiddleSixth: "Top middle sixth",
  topRightSixth: "Top right sixth",
  bottomLeftSixth: "Bottom left sixth",
  bottomMiddleSixth: "Bottom middle sixth",
  bottomRightSixth: "Bottom right sixth",
};

watch(isEnabled, async () => {
  let action!: () => Promise<{ ok: boolean }>;
  if (isEnabled.value) {
    action = api.resume;
  } else {
    action = api.pause;
  }

  await action().catch((e) => {
    console.error(e);
    isEnabled.value = !isEnabled.value;
  });
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
            action: k as Action,
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
      inputValues[k as Action] = v;
    });
  });
  await api.getShortcutsActions().then((shortcutsActions: ShortcutAction[]) => {
    shortcutsActions.forEach(({ action, shortcut }) => {
      inputValues[action] = shortcut;
    });
  });
  await fetchOffsets();

  isInitialized.value = true;
});
</script>

<template>
  <div v-if="isInitialized" class="flex flex-col gap-2 [&>:not(hr)]:mx-4 mt-2">
    <label class="flex gap-2 items-center">
      <span>Enabled</span>
      <Toggle v-model="isEnabled" v-model:enabled="isEnabled" />
    </label>
    <hr />
    <div>
      <fieldset class="grid gap-4 offset-grid w-36">
        <legend style="grid-area: label">Offsets</legend>
        <label class="flex gap-2" style="grid-area: offset-top">
          <span>Top</span>
          <input
            class="px-1 py-0.5 border text-right max-w-20"
            type="number"
            step="1"
            v-model="offsets.top"
          />
        </label>
        <label class="flex gap-2" style="grid-area: offset-left">
          <span>Left</span>
          <input
            class="px-1 py-0.5 border text-right max-w-20"
            type="number"
            step="1"
            v-model="offsets.left"
          />
        </label>
        <label class="flex gap-2" style="grid-area: offset-bottom">
          <span>Bottom</span>
          <input
            class="px-1 py-0.5 border text-right max-w-20"
            type="number"
            step="1"
            v-model="offsets.bottom"
          />
        </label>
        <label class="flex gap-2" style="grid-area: offset-right">
          <span>Right</span>
          <input
            class="px-1 py-0.5 border text-right max-w-20"
            type="number"
            step="1"
            v-model="offsets.right"
          />
        </label>
      </fieldset>
    </div>
    <hr />
    <fieldset class="flex flex-col gap-2">
      <legend>Shortcuts</legend>
      <div class="grid grid-cols-[min-content_min-content_200px] gap-2">
        <template v-for="action of availableActions" :key="action">
          <label :for="`action-${action}`" class="text-nowrap">
            {{ translateActions[action] }}
          </label>
          <WindowIcon :action="action" />
          <ShortcutRecorder
            :name="`action-${action}`"
            v-model="inputValues[action]"
          />
        </template>
      </div>
    </fieldset>
  </div>
</template>

<style scoped>
.offset-grid {
  grid-template-areas:
    "label label label"
    ". offset-top ."
    "offset-left . offset-right"
    ". offset-bottom .";
}
</style>
