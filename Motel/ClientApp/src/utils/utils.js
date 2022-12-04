export const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

export const objectMap = (obj, fn) =>
	Object.fromEntries(
		Object.entries(obj).map(
			([ k, v ], i) => [ k, fn(v, k, i) ]
		)
	)

export function omit(obj, key) {
	const { [key]: omitted, ...rest } = obj;
	return rest;
}
