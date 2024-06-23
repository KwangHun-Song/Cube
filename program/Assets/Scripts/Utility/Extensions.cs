using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility {
    public static class Extensions {
        public static T PickRandom<T>(this IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection), "Collection cannot be null.");
            }

            if (collection.Any() == false) {
                throw new InvalidOperationException("Collection is empty; cannot pick a random item.");
            }

            // Pick a random index
            int randomIndex = UnityEngine.Random.Range(0, collection.Count());

            // Return the element at the random index
            return collection.ElementAt(randomIndex);
        }

        public static bool ItemsAreSame<T>(this IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection), "Collection cannot be null.");
            }

            if (collection.Any() == false) {
                throw new InvalidOperationException("Collection is empty; cannot pick a random item.");
            }
            
            if (collection.Distinct().Skip(1).Any()) return false;
            return true;
        }
    }
}