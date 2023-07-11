namespace MasterProject.GodfieldLight {

    public class Attack {

        public int instigatorPlayerIndex;
        public int damage;
        public float hitProbability;
        public bool lethalIfUnblocked;
        public IReadOnlyList<int> remainingTargetPlayerIndices;

        public Attack? GetResultWithOneFewerTargetPlayers () {
            if (remainingTargetPlayerIndices.Count <= 1) {
                return null;
            }
            var newIndices = new int[remainingTargetPlayerIndices.Count - 1];
            for (int i = 0; i < newIndices.Length; i++) {
                newIndices[i] = remainingTargetPlayerIndices[i + 1];
            }
            return new Attack() {
                instigatorPlayerIndex = this.instigatorPlayerIndex,
                damage = this.damage,
                hitProbability = this.hitProbability,
                lethalIfUnblocked = this.lethalIfUnblocked,
                remainingTargetPlayerIndices = newIndices,
            };
        }

    }

}
