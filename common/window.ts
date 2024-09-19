import { z } from "zod";

export const availableActionNames = [
  "fullScreen",
  "leftOneThird",
  "leftTwoThirds",
  "rightOneThird",
  "rightTwoThirds",
  "leftHalf",
  "dynamicLeftHalf",
  "dynamicRightHalf",
  "rightHalf",
  "topLeftSixth",
  "topMiddleSixth",
  "topRightSixth",
  "bottomLeftSixth",
  "bottomMiddleSixth",
  "bottomRightSixth",
] as const;

export type Action = (typeof availableActionNames)[number];

export const actionValidator = z.enum(availableActionNames);

export const offsetsValidator = z.object({
  top: z.number(),
  left: z.number(),
  bottom: z.number(),
  right: z.number(),
});

export type Offsets = z.output<typeof offsetsValidator>;
