<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, onMounted, provide} from 'vue';
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get } from '@/utils/api';

const router = useRouter();

const roomCleanings = ref([]);
const rooms = ref([]);
provide('roomCleanings', roomCleanings);
provide('rooms', rooms);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	roomCleanings.value = await call_get(`/api/room-cleanings`, { verbose: true })
	rooms.value = await call_get(`/api/rooms`)
	isLoading.value = false;
});
</script>
