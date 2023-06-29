namespace MasterProject.Chess {

    public static class BitMaskUtils {

        public static bool GetLongBit (long longValue, int index) {
            return ((longValue >> index) & 1) == 1;
        }

        public static long SetLongBit (long longValue, int index, bool value) {
            return (value ? (longValue | (1L << index)) : (longValue & ~(1L << index)));
        }

    }

}
