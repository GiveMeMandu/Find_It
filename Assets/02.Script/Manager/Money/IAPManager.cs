// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Purchasing;
// using UnityEngine.Purchasing.Extension;

// namespace Manager
// {
//     [System.Serializable]
//     public class ProductInfo
//     {
//         public string productId;
//         public ProductType productType;
//         public decimal defaultPrice;
//         public string description;
//     }

//     public class IAPManager : MMSingleton<IAPManager>, IDetailedStoreListener
//     {
//         private IStoreController storeController;
//         private IExtensionProvider storeExtensionProvider;
        
//         // 구매 완료 이벤트
//         public event EventHandler<string> OnPurchaseCompleted;

//         // 상품 목록 설정
//         [SerializeField] private List<ProductInfo> productList = new List<ProductInfo>
//         {
//             // 캐시 상품
//             new ProductInfo { productId = "cash1", productType = ProductType.Consumable, defaultPrice = 2000m, description = "2,000원 상품" },
//             new ProductInfo { productId = "cash2", productType = ProductType.Consumable, defaultPrice = 10000m, description = "10,000원 상품" },
//             new ProductInfo { productId = "cash3", productType = ProductType.Consumable, defaultPrice = 50000m, description = "50,000원 상품" },
            
//             // 골드 상품
//             new ProductInfo { productId = "gold1", productType = ProductType.Consumable, defaultPrice = 2000m, description = "골드 2,000원 상품" },
//             new ProductInfo { productId = "gold2", productType = ProductType.Consumable, defaultPrice = 10000m, description = "골드 10,000원 상품" },
//             new ProductInfo { productId = "gold3", productType = ProductType.Consumable, defaultPrice = 50000m, description = "골드 50,000원 상품" },
            
//             // 광고 제거 상품
//             new ProductInfo { productId = "noads", productType = ProductType.NonConsumable, defaultPrice = 5000m, description = "광고 제거 상품" }
//         };

//         void Start()
//         {
//             InitializePurchasing();
//         }

//         void InitializePurchasing()
//         {
//             if (IsInitialized()) return;

//             Debug.Log("IAP 초기화 시작...");
//             var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

//             // 모든 상품 등록
//             foreach (var product in productList)
//             {
//                 builder.AddProduct(product.productId, product.productType);
//                 Debug.Log($"상품 등록: {product.productId}, 타입: {product.productType}, 기본 가격: {product.defaultPrice}");
//             }

//             UnityPurchasing.Initialize(this, builder);
//         }

//         private bool IsInitialized()
//         {
//             return storeController != null && storeExtensionProvider != null;
//         }

//         public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//         {
//             storeController = controller;
//             storeExtensionProvider = extensions;
//             Debug.Log("IAP 초기화 성공!");
            
//             // 초기화된 모든 상품 정보 출력
//             Debug.Log("=== 초기화된 상품 목록 ===");
//             foreach (var product in storeController.products.all)
//             {
//                 Debug.Log($"상품: {product.definition.id}, 타입: {product.definition.type}, 가격: {product.metadata.localizedPriceString}");
//             }
//             Debug.Log("========================");
//         }

//         public void OnInitializeFailed(InitializationFailureReason error)
//         {
//             Debug.LogError($"IAP 초기화 실패: {error}");
//         }

//         public void OnInitializeFailed(InitializationFailureReason error, string message)
//         {
//             Debug.LogError($"IAP 초기화 실패: {error}, 메시지: {message}");
//         }

//         public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
//         {
//             // 구매 성공 처리
//             var product = purchaseEvent.purchasedProduct;
//             Debug.Log($"상품 구매 성공: {product.definition.id}");

//             try
//             {
//                 // 이벤트 핸들러가 등록되어 있는지 확인
//                 bool hasHandlers = OnPurchaseCompleted != null;
//                 Debug.Log($"구매 완료 이벤트 핸들러 등록 여부: {hasHandlers}");
                
//                 // 이벤트 발생
//                 OnPurchaseCompleted?.Invoke(this, product.definition.id);
//                 Debug.Log($"구매 완료 이벤트 발생: {product.definition.id}");
                
//                 // 데이터 즉시 저장
//                 if (Global.UserDataManager != null)
//                 {
//                     Global.UserDataManager.Save();
//                     Debug.Log("구매 후 데이터 저장 완료");
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"구매 처리 중 오류 발생: {e.Message}");
//             }
            
//             return PurchaseProcessingResult.Complete;
//         }

//         public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
//         {
//             Debug.LogError($"상품 구매 실패: {product.definition.id}, 사유: {failureDescription.reason}, 메시지: {failureDescription.message}");
//         }

//         public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//         {
//             Debug.LogError($"상품 구매 실패: {product.definition.id}, 사유: {failureReason}");
//         }

//         public void OnStoreConfiguration(ProductCollection products)
//         {
//             Debug.Log("스토어 설정 완료");
//         }

//         // 상품 구매 시도 메서드
//         public void BuyProduct(string productId)
//         {
//             if (!IsInitialized())
//             {
//                 Debug.LogError("IAP가 초기화되지 않았습니다!");
//                 return;
//             }

//             Product product = storeController.products.WithID(productId);
//             if (product != null && product.availableToPurchase)
//             {
//                 Debug.Log($"상품 구매 시도: {product.definition.id}");
//                 storeController.InitiatePurchase(product);
//             }
//             else
//             {
//                 Debug.LogError($"상품을 찾을 수 없거나 구매할 수 없습니다: {productId}");
//             }
//         }

//         // 현지화된 가격 가져오기
//         public string GetLocalizedPrice(string productId)
//         {
//             if (!IsInitialized()) 
//             {
//                 Debug.LogWarning("IAP가 초기화되지 않았습니다. 기본 가격을 사용합니다.");
//                 return GetDefaultPriceString(productId);
//             }

//             Product product = storeController.products.WithID(productId);
//             if (product != null && product.availableToPurchase)
//             {
//                 var price = string.Format("{0} {1}", product.metadata.localizedPrice, product.metadata.isoCurrencyCode);
                
//                 // 가격이 0인 경우 기본 가격 사용
//                 if (product.metadata.localizedPrice == 0)
//                 {
//                     Debug.LogWarning($"상품 가격이 0입니다: {productId}, 기본 가격을 사용합니다.");
//                     return GetDefaultPriceString(productId);
//                 }
                
//                 return price;
//             }
//             else
//             {
//                 Debug.LogWarning($"상품을 찾을 수 없습니다: {productId}, 기본 가격을 사용합니다.");
//                 return GetDefaultPriceString(productId);
//             }
//         }
        
//         // 상품의 기본 가격 문자열 반환
//         public string GetDefaultPriceString(string productId)
//         {
//             var product = productList.Find(p => p.productId == productId);
//             if (product != null)
//             {
//                 return $"₩ {product.defaultPrice:N0}";
//             }
            
//             Debug.LogWarning($"기본 가격 정보를 찾을 수 없음: {productId}");
//             return "₩ 0";
//         }
        
//         // 상품의 기본 가격 반환
//         public decimal GetDefaultPrice(string productId)
//         {
//             var product = productList.Find(p => p.productId == productId);
//             if (product != null)
//             {
//                 return product.defaultPrice;
//             }
            
//             Debug.LogWarning($"기본 가격 정보를 찾을 수 없음: {productId}");
//             return 0m;
//         }
        
//         // 상품 정보 가져오기
//         public Product GetProduct(string productId)
//         {
//             if (!IsInitialized()) return null;
//             return storeController.products.WithID(productId);
//         }
//     }
// }