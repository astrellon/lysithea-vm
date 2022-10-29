export type Editable<T> =
{
    -readonly [P in keyof T]: T[P];
};
