import { expect, test } from 'vitest'
import { getGasFeeOption } from './NetworkUtils'

test('getGasFeeOption adds 20% buffer to the gas estimate', () => {
    const input = 1000n;
    const result = getGasFeeOption(input);
    expect(result.gasLimit).toBe(1200n);
})
