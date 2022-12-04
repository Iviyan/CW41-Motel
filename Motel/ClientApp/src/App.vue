<template>
	<router-view />
	<Icons />
</template>

<script setup>
import { watch, onErrorCaptured } from 'vue';
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import iziToast from "izitoast";
import { RequestError } from "@/exceptions";
import Icons from "@/components/Icons";

const store = useStore();
const router = useRouter();

onErrorCaptured((err, vm, info) => {
	console.log(`error: ${ err.toString() }\ninfo: ${ info }`);
	if (err instanceof RequestError) {
		if (err.rfc7807) {
			iziToast.error({
				title: 'Ошибка запроса.',
				message: err.message,
				layout: 2,
				timeout: 4000,
				class: "iziToast-api-error"
			});
		} else {
			iziToast.error({
				title: 'Неизвестная ошибка запроса.',
				message: 'Код: ' + err.status,
				timeout: 4000,
				class: "iziToast-api-error"
			});
		}
	}
	//return false;
});

watch(() => store.state.auth.jwt, (newJwt) => {
	if (!newJwt) router.push('/login')
})

</script>

<style>

</style>
