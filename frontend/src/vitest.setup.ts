import { config } from '@vue/test-utils'
import OpenVue from 'openvue/config'
import { i18n } from '@/i18n'

config.global.plugins = [OpenVue, i18n]
